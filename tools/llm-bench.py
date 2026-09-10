# -*- coding: utf-8 -*-
"""STARSTATE llama-server 全链路测速：health / prefill / gen / TTFT / 游戏典型请求。"""
from __future__ import print_function
import json
import time
import urllib.request
import urllib.error

BASE = "http://127.0.0.1:8817"
MODEL = "local"
SYSTEM = (
    "你是人生模拟游戏《STARSTATE》的中文叙事引擎。"
    "若用户任务标注【纯文本】，则只输出纯文本本身，不要JSON、不要代码块。"
)


def http_json(url, payload, timeout=180):
    data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={"Content-Type": "application/json", "Accept": "application/json"},
        method="POST",
    )
    t0 = time.perf_counter()
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        body = resp.read()
    dt = time.perf_counter() - t0
    return dt, json.loads(body.decode("utf-8"))


def http_get(url, timeout=5):
    t0 = time.perf_counter()
    try:
        with urllib.request.urlopen(url, timeout=timeout) as resp:
            code = resp.status
            resp.read()
        return time.perf_counter() - t0, code, None
    except Exception as e:
        return time.perf_counter() - t0, None, str(e)


def stream_chat(payload, timeout=180):
    """返回 (ttft, total, content, reason, n_chunks, usage_like)"""
    data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    req = urllib.request.Request(
        BASE + "/v1/chat/completions",
        data=data,
        headers={
            "Content-Type": "application/json",
            "Accept": "text/event-stream",
        },
        method="POST",
    )
    t0 = time.perf_counter()
    ttft = None
    content = []
    reason = []
    n = 0
    usage = None
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        raw = resp.read().decode("utf-8", "replace")
    total = time.perf_counter() - t0
    # llama-server 可能把 SSE 打成一整块；按行拆
    for line in raw.splitlines():
        line = line.strip()
        if not line.startswith("data:"):
            continue
        p = line[5:].strip()
        if p == "[DONE]":
            continue
        try:
            obj = json.loads(p)
        except Exception:
            continue
        if ttft is None:
            # 第一个含 text 的 chunk 才算首 token
            ch = (obj.get("choices") or [{}])[0]
            d = ch.get("delta") or {}
            piece = d.get("content") or d.get("reasoning_content") or ""
            if piece:
                ttft = time.perf_counter() - t0
        ch = (obj.get("choices") or [{}])[0]
        d = ch.get("delta") or {}
        c = d.get("content") or ""
        r = d.get("reasoning_content") or ""
        if c:
            content.append(c)
        if r:
            reason.append(r)
        if c or r:
            n += 1
        if obj.get("usage"):
            usage = obj["usage"]
    if ttft is None:
        ttft = total
    return ttft, total, "".join(content), "".join(reason), n, usage


def make_prompt(n_chars, seed="长安市发展和改革局综合科的台账口径"):
    # 中文填充，接近游戏 prompt 分布
    pad = ("综合科今日核对项目审批链条与会议纪要对应关系，"
           "数字不会说话但会留痕。" )
    s = seed
    while len(s) < n_chars:
        s += pad
    return s[:n_chars]


def bench_case(name, user, max_tokens=200, stream=False, temperature=0.7):
    payload = {
        "model": MODEL,
        "messages": [
            {"role": "system", "content": SYSTEM},
            {"role": "user", "content": user},
        ],
        "temperature": temperature,
        "max_tokens": max_tokens,
        "stream": stream,
    }
    if stream:
        ttft, total, content, reason, nchunks, usage = stream_chat(payload)
        pt = (usage or {}).get("prompt_tokens")
        ct = (usage or {}).get("completion_tokens")
        prefill = (pt / ttft) if pt and ttft > 0 else None
        gen = None
        if ct and total > ttft:
            gen = ct / (total - ttft)
        elif ct and total > 0:
            gen = ct / total
        return {
            "name": name,
            "mode": "stream",
            "user_chars": len(user),
            "prompt_tokens": pt,
            "completion_tokens": ct,
            "ttft_s": round(ttft, 3),
            "total_s": round(total, 3),
            "prefill_tps": round(prefill, 1) if prefill else None,
            "gen_tps": round(gen, 1) if gen else None,
            "chunks": nchunks,
            "content_len": len(content),
            "reason_len": len(reason),
            "content_head": content[:60].replace("\n", " "),
        }
    t, obj = http_json(BASE + "/v1/chat/completions", payload)
    ch = (obj.get("choices") or [{}])[0]
    msg = ch.get("message") or {}
    usage = obj.get("usage") or {}
    pt = usage.get("prompt_tokens")
    ct = usage.get("completion_tokens")
    # 非流式：prefill+gen 混在总时长；用 usage 估
    # 假设 prefill 按历史标定 ~70t/s 会失真——直接报 wall 与均速
    wall_tps = (ct / t) if ct and t > 0 else None
    content = msg.get("content") or ""
    reason = msg.get("reasoning_content") or ""
    return {
        "name": name,
        "mode": "nostream",
        "user_chars": len(user),
        "prompt_tokens": pt,
        "completion_tokens": ct,
        "wall_s": round(t, 3),
        "wall_tps": round(wall_tps, 1) if wall_tps else None,
        "finish": ch.get("finish_reason"),
        "content_len": len(content),
        "reason_len": len(reason),
        "content_head": content[:60].replace("\n", " "),
    }


def main():
    report = {"base": BASE, "cases": []}

    # 1) health
    for i in range(3):
        dt, code, err = http_get(BASE + "/health")
        report.setdefault("health_ms", []).append(round(dt * 1000, 1))
        if err:
            report["health_err"] = err
            print("HEALTH FAIL", err)
            return
    print("health_ms:", report["health_ms"])

    # 2) prefill 梯度（短 max_tokens，看 prompt 处理）
    sizes = [200, 800, 2000, 4000]
    for sz in sizes:
        user = make_prompt(sz) + "\n【任务】用一句中文总结这件事，不超过30字。【纯文本】"
        r = bench_case("prefill_%dchars" % sz, user, max_tokens=40, stream=False, temperature=0.1)
        report["cases"].append(r)
        print("PRE", r)

    # 3) 生成速度（短 prompt 长输出）
    r = bench_case(
        "gen_long",
        "写一段 150 字左右的机关走廊日常，克制写实。【纯文本】",
        max_tokens=220,
        stream=False,
        temperature=0.8,
    )
    report["cases"].append(r)
    print("GEN", r)

    # 4) 流式 TTFT / gen（典型短输出）
    r = bench_case(
        "stream_short",
        "写周五下班时的机关走廊一句话，不超过40字。【纯文本】",
        max_tokens=120,
        stream=True,
        temperature=0.7,
    )
    report["cases"].append(r)
    print("ST", r)

    # 5) 游戏典型请求（贴近 LlmPrompt）
    talk_user = (
        "【当前】2027-05-04 星期三（工作日）；玩家：沈知行，吏三·科员，综合科，士气62，压力41。\n"
        "【交谈对象】周衡之，综合科科长（吏一·正科），45岁，性格：严谨、护短。\n"
        "【双方关系】熟悉20，信任6，评价3。\n"
        "【任务】写一段此刻两人的日常交谈。输出 JSON："
        "{\"greeting\":\"开场\",\"lines\":[\"后续1\",\"后续2\"],"
        "\"options\":[{\"label\":\"玩家回应\",\"mood\":\"warm\",\"reply\":\"对方\"},"
        "{\"label\":\"回应2\",\"mood\":\"neutral\",\"reply\":\"对方\"}]}。options 恰好2个。"
    )
    r = bench_case("game_talk_json", talk_user, max_tokens=720, stream=False, temperature=0.9)
    report["cases"].append(r)
    print("TALK", r)

    r = bench_case(
        "game_talk_stream",
        talk_user,
        max_tokens=720,
        stream=True,
        temperature=0.9,
    )
    report["cases"].append(r)
    print("TALK_ST", r)

    letter_user = (
        "【日期】2027-03-06 星期日；玩家：沈知行，吏三·科员；住房公租房，单身；士气58，压力44。【纯文本】\n"
        "【任务】以父母口吻写一封家信，不超过120字。"
    )
    r = bench_case("game_letter", letter_user, max_tokens=360, stream=False, temperature=0.8)
    report["cases"].append(r)
    print("LETTER", r)

    r = bench_case(
        "game_letter_stream",
        letter_user,
        max_tokens=360,
        stream=True,
        temperature=0.8,
    )
    report["cases"].append(r)
    print("LETTER_ST", r)

    micro_user = (
        "【日期】2027-04-12；玩家：沈知行，吏三·科员，综合科；精力70，压力35。\n"
        "【任务】写一件机关里的不起眼小事。输出 JSON："
        "{\"title\":\"标题\",\"paras\":[\"第一段\",\"第二段\"],"
        "\"options\":[{\"label\":\"办妥\",\"mood\":\"good\",\"result\":\"结果\"},"
        "{\"label\":\"按部就班\",\"mood\":\"neutral\",\"result\":\"结果\"}]}。options 恰好2个。"
    )
    r = bench_case("game_micro_json", micro_user, max_tokens=460, stream=False, temperature=0.9)
    report["cases"].append(r)
    print("MICRO", r)

    # 汇总
    print("\n===== SUMMARY =====")
    ns = [c for c in report["cases"] if c.get("mode") == "nostream" and c.get("prompt_tokens")]
    if ns:
        # 用最短与最长 prompt 估 prefill 斜率
        ns_sorted = sorted(ns, key=lambda x: x["prompt_tokens"])
        a, b = ns_sorted[0], ns_sorted[-1]
        if b["prompt_tokens"] > a["prompt_tokens"] and b["wall_s"] > a["wall_s"]:
            dpt = b["prompt_tokens"] - a["prompt_tokens"]
            dts = b["wall_s"] - a["wall_s"]
            if dts > 0:
                est_prefill = dpt / dts
                print("est_prefill_tps_from_slope:", round(est_prefill, 1),
                      "between", a["name"], b["name"])
                report["est_prefill_tps_from_slope"] = round(est_prefill, 1)

    out = "E:/Starstate/tmpbuild/llm-bench.json"
    with open(out, "wb") as f:
        f.write(json.dumps(report, ensure_ascii=False, indent=2).encode("utf-8"))
    print("written:", out)


if __name__ == "__main__":
    main()
