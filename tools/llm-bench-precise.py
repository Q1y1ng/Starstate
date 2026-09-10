# -*- coding: utf-8 -*-
"""精确测速：边读 SSE 边计首 token；prefill 用 max_tokens=1 隔离。"""
from __future__ import print_function
import http.client
import json
import time

HOST = "127.0.0.1"
PORT = 8817
SYSTEM = "你是《STARSTATE》中文叙事引擎。【纯文本】任务只输出纯文本。"


def post_json(path, payload, timeout=180):
    conn = http.client.HTTPConnection(HOST, PORT, timeout=timeout)
    body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    t0 = time.perf_counter()
    conn.request("POST", path, body=body, headers={"Content-Type": "application/json"})
    resp = conn.getresponse()
    data = resp.read()
    dt = time.perf_counter() - t0
    conn.close()
    return resp.status, dt, json.loads(data.decode("utf-8"))


def stream_sse(payload, timeout=180):
    conn = http.client.HTTPConnection(HOST, PORT, timeout=timeout)
    body = json.dumps(payload, ensure_ascii=False).encode("utf-8")
    headers = {
        "Content-Type": "application/json",
        "Accept": "text/event-stream",
    }
    t0 = time.perf_counter()
    conn.request("POST", "/v1/chat/completions", body=body, headers=headers)
    resp = conn.getresponse()
    ttft = None
    content = []
    n = 0
    buf = b""
    while True:
        chunk = resp.read(256)
        if not chunk:
            break
        buf += chunk
        while b"\n" in buf:
            line, buf = buf.split(b"\n", 1)
            line = line.decode("utf-8", "replace").strip()
            if not line.startswith("data:"):
                continue
            p = line[5:].strip()
            if p == "[DONE]":
                continue
            try:
                obj = json.loads(p)
            except Exception:
                continue
            ch = (obj.get("choices") or [{}])[0]
            d = ch.get("delta") or {}
            piece = d.get("content") or ""
            if piece:
                if ttft is None:
                    ttft = time.perf_counter() - t0
                content.append(piece)
                n += 1
    total = time.perf_counter() - t0
    conn.close()
    text = "".join(content)
    return {
        "ttft_s": None if ttft is None else round(ttft, 3),
        "total_s": round(total, 3),
        "chunks": n,
        "content_len": len(text),
        "content_head": text[:80].replace("\n", " "),
    }


def pad(n):
    s = "综合科核对项目审批链条与会议纪要对应关系，数字不会说话但会留痕。"
    while len(s) < n:
        s += s
    return s[:n]


def main():
    out = []

    # health x5
    hs = []
    for _ in range(5):
        conn = http.client.HTTPConnection(HOST, PORT, timeout=5)
        t0 = time.perf_counter()
        conn.request("GET", "/health")
        r = conn.getresponse()
        r.read()
        hs.append(round((time.perf_counter() - t0) * 1000, 1))
        conn.close()
    print("health_ms", hs)
    out.append({"health_ms": hs})

    # prefill isolation: max_tokens=1
    print("\n--- prefill (max_tokens=1) ---")
    prefill_rows = []
    for sz in (100, 400, 1000, 2000, 4000, 8000):
        user = pad(sz) + " 用不超过20字总结。"
        payload = {
            "model": "local",
            "messages": [
                {"role": "system", "content": SYSTEM},
                {"role": "user", "content": user},
            ],
            "temperature": 0.0,
            "max_tokens": 1,
            "stream": False,
        }
        status, dt, obj = post_json("/v1/chat/completions", payload)
        usage = obj.get("usage") or {}
        pt = usage.get("prompt_tokens") or 0
        ct = usage.get("completion_tokens") or 0
        # 1 token gen 开销忽略，近似 prefill
        tps = (pt / dt) if dt > 0 and pt else None
        row = {
            "user_chars": sz,
            "prompt_tokens": pt,
            "completion_tokens": ct,
            "wall_s": round(dt, 3),
            "prefill_tps": round(tps, 1) if tps else None,
        }
        prefill_rows.append(row)
        print(row)
    out.append({"prefill": prefill_rows})

    # generation: short prompt, long max_tokens
    print("\n--- generation ---")
    gen_rows = []
    for mt in (50, 150, 300):
        payload = {
            "model": "local",
            "messages": [
                {"role": "system", "content": SYSTEM},
                {"role": "user", "content": "写一段机关走廊日常，克制写实。【纯文本】"},
            ],
            "temperature": 0.8,
            "max_tokens": mt,
            "stream": False,
        }
        status, dt, obj = post_json("/v1/chat/completions", payload)
        usage = obj.get("usage") or {}
        pt = usage.get("prompt_tokens") or 0
        ct = usage.get("completion_tokens") or 0
        # wall ≈ prefill(短) + gen；用 prefill~70t/s 估 prefill 时间后拆
        # 直接报 wall 与 overall；另用 ct/(dt - pt/est) 估 gen
        est_pre = pt / 80.0
        gen_t = max(dt - est_pre, 0.01)
        row = {
            "max_tokens": mt,
            "prompt_tokens": pt,
            "completion_tokens": ct,
            "wall_s": round(dt, 3),
            "overall_tps": round(ct / dt, 1) if dt > 0 and ct else None,
            "est_gen_tps": round(ct / gen_t, 1) if ct else None,
            "finish": (obj.get("choices") or [{}])[0].get("finish_reason"),
        }
        gen_rows.append(row)
        print(row)
    out.append({"generation": gen_rows})

    # true TTFT stream
    print("\n--- stream TTFT ---")
    st_rows = []
    cases = [
        ("short_40", "写周五下班时的机关走廊一句话，不超过40字。【纯文本】", 80),
        ("letter_120", "以父母口吻写一封家信，不超过120字。【纯文本】", 200),
        (
            "talk_json",
            "写两人日常交谈 JSON：{\"greeting\":\"a\",\"lines\":[\"b\",\"c\"],\"options\":[{\"label\":\"x\",\"mood\":\"warm\",\"reply\":\"y\"},{\"label\":\"x2\",\"mood\":\"neutral\",\"reply\":\"y2\"}]}",
            400,
        ),
        ("micro_json", "写小事 JSON title/paras/options，options 恰好2个。", 300),
    ]
    for name, user, mt in cases:
        payload = {
            "model": "local",
            "messages": [
                {"role": "system", "content": SYSTEM},
                {"role": "user", "content": user},
            ],
            "temperature": 0.8,
            "max_tokens": mt,
            "stream": True,
        }
        r = stream_sse(payload)
        r["name"] = name
        r["max_tokens"] = mt
        # gen tps ≈ content_len/4 / (total-ttft) 粗估（中文约 1字~1token 或 0.6）
        if r["ttft_s"] is not None and r["total_s"] > r["ttft_s"]:
            est_tok = max(r["content_len"] // 2, 1)
            r["est_gen_tps"] = round(est_tok / (r["total_s"] - r["ttft_s"]), 1)
        st_rows.append(r)
        print(r)
    out.append({"stream": st_rows})

    # 二次 prefill 斜率（排除首请求缓存）
    if len(prefill_rows) >= 3:
        a = prefill_rows[1]
        b = prefill_rows[-1]
        dpt = b["prompt_tokens"] - a["prompt_tokens"]
        dts = b["wall_s"] - a["wall_s"]
        slope = round(dpt / dts, 1) if dts > 0 else None
        print("\nest_prefill_tps_slope_400_to_max:", slope)
        out.append({"est_prefill_tps_slope": slope})

    path = "E:/Starstate/tmpbuild/llm-bench-precise.json"
    with open(path, "wb") as f:
        f.write(json.dumps(out, ensure_ascii=False, indent=2).encode("utf-8"))
    print("written", path)


if __name__ == "__main__":
    main()
