# -*- coding: utf-8 -*-
"""STARSTATE 内容占比审计：统计事件/新闻/任务模板独特度与规模。"""
from __future__ import print_function
import os, re, sys, json

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
SRC = os.path.join(ROOT, "game-src", "Assets", "Scripts")

def read(p):
    with open(p, "rb") as f:
        return f.read().decode("utf-8", "replace")

def main():
    core = os.path.join(SRC, "Core")
    ui = os.path.join(SRC, "Ui")
    files = []
    for d in (core, ui):
        if not os.path.isdir(d):
            continue
        for name in os.listdir(d):
            if name.endswith(".cs"):
                files.append(os.path.join(d, name))

    all_text = []
    event_ids = []
    titles = []
    paras = []
    news_lines = []
    templates = 0

    for p in files:
        t = read(p)
        all_text.append(t)
        event_ids += re.findall(r'id\s*=\s*"([^"]+)"', t)
        titles += re.findall(r'title\s*=\s*"([^"]+)"', t)
        # 段落字符串（粗匹配）
        paras += re.findall(r'"([^"\n]{20,})"', t)
        if "News.Add" in t:
            news_lines += re.findall(r'News\.Add\(', t)
        if "new Template" in t:
            templates += t.count("new Template")

    # 过滤非事件 id
    content_ids = [i for i in event_ids if not i.startswith("t_") and not i.startswith("_")]
    unique_ids = set(content_ids)
    unique_titles = set(titles)
    unique_paras = set(paras)

    report = {
        "cs_files": len(files),
        "content_event_ids": len(content_ids),
        "unique_event_ids": len(unique_ids),
        "dup_event_ids": sorted([i for i in unique_ids if content_ids.count(i) > 1])[:20],
        "titles": len(titles),
        "unique_titles": len(unique_titles),
        "long_strings": len(paras),
        "unique_long_strings": len(unique_paras),
        "unique_string_ratio": round(len(unique_paras) / float(max(1, len(paras))), 3),
        "news_add_calls": len(news_lines),
        "task_templates": templates,
        "total_lines": sum(t.count("\n") + 1 for t in all_text),
    }

    out = os.path.join(ROOT, "tmpbuild", "content-audit.json")
    try:
        os.makedirs(os.path.dirname(out), exist_ok=True)
    except Exception:
        pass
    data = json.dumps(report, ensure_ascii=False, indent=2)
    with open(out, "wb") as f:
        f.write(data.encode("utf-8"))
    print(data)
    print("written:", out)

if __name__ == "__main__":
    main()
