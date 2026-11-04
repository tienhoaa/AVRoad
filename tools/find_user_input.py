import json

with open(r'C:\Users\PC\.gemini\antigravity\brain\4bae2500-d3e0-440e-a0f3-6b918cb91ce7\.system_generated\logs\transcript.jsonl', 'r', encoding='utf-8') as f:
    with open('user_inputs.txt', 'w', encoding='utf-8') as out:
        for line in f:
            data = json.loads(line)
            if data.get('type') == 'USER_INPUT':
                content = data.get('content', '')
                for k in ['c:\\', 'd:\\', 'program files', 'cài', 'duong dan', 'đường dẫn', 'gốc']:
                    if k in content.lower():
                        idx = data.get('step_index')
                        out.write(f"STEP {idx}:\n{content}\n====================\n")
                        break
print("Done")
