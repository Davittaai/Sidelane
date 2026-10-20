"""Source-only guardrail; it does NOT compile or run the WPF application."""
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]
SRC = ROOT / 'src' / 'Sidelane'
valid = True
for xaml in SRC.glob('*.xaml'):
    ET.parse(xaml)
    codebehind = SRC / (xaml.name + '.cs')
    text = codebehind.read_text(encoding='utf-8') if codebehind.exists() else ''
    handlers = re.findall(r'(?:Click|KeyDown|MouseEnter|MouseLeave|Deactivated|Closing|Loaded|ValueChanged|TextChanged|SelectionChanged|PreviewMouseLeftButtonDown)="([A-Za-z_]\w*)"', xaml.read_text(encoding='utf-8'))
    missing = sorted(set(h for h in handlers if h not in text))
    valid = valid and not missing
    print(f'{"PASS" if not missing else "FAIL"} XML and handler references: {xaml.name}',missing)
for required in ('LICENSE','THIRD_PARTY_NOTICES.md','SECURITY.md','CONTRIBUTING.md','.github/workflows/windows.yml'):
    present=(ROOT/required).is_file()
    valid=valid and present
    print(f'{"PASS" if present else "FAIL"} required file: {required}')
if not valid: raise SystemExit(1)
