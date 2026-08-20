# Halli layout contract 0.4.2

Final acceptance source: GitHub issue #54.

- Show at most two cards in each side pile.
- Draw the newest revealed card last and on top.
- Keep only the immediately previous card underneath.
- Use `CardWidth = 64` and `CardStepX = 59` (5 px overlap, 7.8125%).
- Do not restore `Halli_0_4_1`; it is an archived conflicting three-card contract.
- This package changes no runtime PNG. It locks the layout contract used by the existing card art.

Before merging Halli UI work, run:

```powershell
& programer/CodexGame/ArtReferences/RuntimeBindingTools/audit_runtime_art_bindings_0_6_2.ps1
```
