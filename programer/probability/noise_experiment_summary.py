import pandas as pd

# This helper keeps the notebook output readable and can be referenced from a later cell.

if 'results_df' in globals():
    summary = results_df.copy()
    summary.columns = ['노이즈 강도', '정확도', '정밀도', '재현율', 'F1 점수']
    summary = summary.round(4)
    print('노이즈 강도별 평가 결과')
    print(summary.to_string(index=False))
