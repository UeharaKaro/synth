#!/usr/bin/env python3
"""
Pattern Radar Chart Generator
8각형 레이더 차트 이미지 생성 (사운드 볼텍스 스타일)
"""

import matplotlib.pyplot as plt
import numpy as np
from matplotlib.patches import Polygon

# 한글 폰트 설정 (Windows)
try:
    plt.rcParams['font.family'] = 'Malgun Gothic'  # Windows
except:
    try:
        plt.rcParams['font.family'] = 'AppleGothic'  # Mac
    except:
        print("Warning: 한글 폰트를 찾을 수 없습니다. 기본 폰트를 사용합니다.")

plt.rcParams['axes.unicode_minus'] = False  # 마이너스 기호 깨짐 방지

def create_radar_chart(values, labels, title="Pattern Radar Chart", filename="radar_chart.png"):
    """
    레이더 차트 생성

    Args:
        values: 각 패턴의 값 리스트 (0-20, 소수점 1자리 지원)
        labels: 패턴 이름 리스트
        title: 차트 제목
        filename: 저장할 파일명
    """
    # 패턴 개수
    num_patterns = len(values)

    # 각도 계산 (위쪽부터 시계방향)
    angles = np.linspace(0, 2 * np.pi, num_patterns, endpoint=False).tolist()

    # 첫 번째 값을 마지막에 추가 (닫힌 다각형)
    values = values + [values[0]]
    angles += angles[:1]

    # 그래프 생성
    fig, ax = plt.subplots(figsize=(10, 10), subplot_kw=dict(projection='polar'))

    # 시작 각도 조정 (위쪽부터 시작)
    ax.set_theta_offset(np.pi / 2)
    ax.set_theta_direction(-1)

    # 격자선 그리기 (0%, 25%, 50%, 75%, 100%)
    max_value = 20
    grid_levels = [0, 5, 10, 15, 20]

    for level in grid_levels:
        if level == 0:
            continue
        ax.plot(angles[:-1], [level] * num_patterns,
                color='white', linewidth=0.5, linestyle=':', alpha=0.3)

    # 데이터 다각형 그리기
    ax.plot(angles, values,
            color='#3399FF', linewidth=3, linestyle='solid', label='Player Skill')
    ax.fill(angles, values,
            color='#3399FF', alpha=0.25)

    # 레이블 설정
    ax.set_xticks(angles[:-1])
    ax.set_xticklabels(labels, size=14, weight='bold', color='white')

    # Y축 설정
    ax.set_ylim(0, max_value)
    ax.set_yticks(grid_levels)
    ax.set_yticklabels([str(v) for v in grid_levels], size=10, color='white', alpha=0.7)

    # Y축 격자선
    ax.yaxis.grid(True, color='white', linestyle=':', linewidth=0.5, alpha=0.3)
    ax.xaxis.grid(True, color='white', linestyle='-', linewidth=1, alpha=0.5)

    # 배경색 설정 (어두운 테마)
    fig.patch.set_facecolor('#1a1a2e')
    ax.set_facecolor('#16213e')
    ax.spines['polar'].set_color('white')
    ax.spines['polar'].set_linewidth(2)

    # 제목
    plt.title(title, size=20, color='white', weight='bold', pad=30)

    # 범례
    plt.legend(loc='upper right', bbox_to_anchor=(1.3, 1.1), fontsize=12)

    # 여백 조정
    plt.tight_layout()

    # 저장
    plt.savefig(filename, dpi=300, facecolor=fig.get_facecolor(), edgecolor='none', bbox_inches='tight')
    print(f"✓ 레이더 차트 저장 완료: {filename}")

    # 화면에 표시 (선택사항)
    # plt.show()

    plt.close()

def main():
    # 영문 라벨 (한글 폰트 문제 해결)
    patterns_eng = ["Trill", "Stairs", "Chord", "Denim", "Jacks", "LN Hybrid", "Burst", "Offbeat"]
    patterns_kr = ["트릴", "계단", "동치", "데님", "따닥이", "롱잡", "폭타", "즈레"]

    # 예시 1: 모든 패턴이 15.0점인 경우
    values_15 = [15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0, 15.0]

    create_radar_chart(
        values=values_15,
        labels=patterns_eng,
        title="Pattern Radar - All 15.0/20.0",
        filename="pattern_radar_15_en.png"
    )

    # 예시 2: 다양한 값 (소수점 포함)
    values_varied = [18.5, 12.0, 16.5, 10.5, 19.0, 14.5, 17.0, 11.5]

    create_radar_chart(
        values=values_varied,
        labels=patterns_eng,
        title="Pattern Radar - Varied Skills",
        filename="pattern_radar_varied_en.png"
    )

    # 예시 3: 최대값 (20.0점)
    values_max = [20.0, 20.0, 20.0, 20.0, 20.0, 20.0, 20.0, 20.0]

    create_radar_chart(
        values=values_max,
        labels=patterns_eng,
        title="Pattern Radar - Perfect 20.0/20.0",
        filename="pattern_radar_max_en.png"
    )

    print("\n✓ 모든 레이더 차트 생성 완료!")
    print("  - pattern_radar_15_en.png (평균 15.0점)")
    print("  - pattern_radar_varied_en.png (다양한 점수, 소수점 포함)")
    print("  - pattern_radar_max_en.png (만점 20.0점)")
    print("\n패턴 타입 (Pattern Types):")
    for eng, kor in zip(patterns_eng, patterns_kr):
        print(f"  {eng:12} - {kor}")

if __name__ == "__main__":
    main()
