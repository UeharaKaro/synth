using UnityEngine;

namespace ChartSystem
{
    /// <summary>
    /// 차트의 패턴별 난이도 속성
    /// 각 패턴 타입에 대해 0-20 스케일로 난이도 평가
    /// </summary>
    [System.Serializable]
    public class PatternDifficulty
    {
        [Header("패턴별 난이도 (0-20 스케일)")]

        [Tooltip("트릴: 두 트랙에 배치된 노트가 일정 간격으로 번갈아 나오는 구조 (교차 연타)")]
        [Range(0, 20)] public float trill = 0f;

        [Tooltip("계단: 노트 배치가 계단을 옆에서 본 것처럼 이루어진 패턴")]
        [Range(0, 20)] public float stairs = 0f;

        [Tooltip("동시치기(동치): 여러 개의 시퀀스를 동시에 치는 패턴")]
        [Range(0, 20)] public float chord = 0f;

        [Tooltip("겹계단(데님): 135-24 / 1357-246 식으로 거미줄처럼 짜인 배치")]
        [Range(0, 20)] public float denim = 0f;

        [Tooltip("따닥이(jacks): 짧은 연타와 잡노트가 섞인 배치, 같은 라인의 노트를 8비트 이상의 고속으로 처리")]
        [Range(0, 20)] public float jacks = 0f;

        [Tooltip("롱잡: 롱노트를 처리하는 중에 다른 노트를 처리해야 하는 패턴")]
        [Range(0, 20)] public float longNoteHybrid = 0f;

        [Tooltip("폭타: 순간적 또는 지속적으로 많은 노트 처리가 필요한 패턴")]
        [Range(0, 20)] public float burst = 0f;

        [Tooltip("즈레/엇박: 노트가 정박에서 약간 어긋나는 패턴")]
        [Range(0, 20)] public float offbeat = 0f;

        // 기본 생성자
        public PatternDifficulty()
        {
            trill = 0f;
            stairs = 0f;
            chord = 0f;
            denim = 0f;
            jacks = 0f;
            longNoteHybrid = 0f;
            burst = 0f;
            offbeat = 0f;
        }

        /// <summary>
        /// 모든 패턴 난이도 초기화
        /// </summary>
        public void Clear()
        {
            trill = 0f;
            stairs = 0f;
            chord = 0f;
            denim = 0f;
            jacks = 0f;
            longNoteHybrid = 0f;
            burst = 0f;
            offbeat = 0f;
        }

        /// <summary>
        /// 평균 패턴 난이도 계산
        /// </summary>
        public float GetAverageDifficulty()
        {
            return (trill + stairs + chord + denim + jacks + longNoteHybrid + burst + offbeat) / 8f;
        }

        /// <summary>
        /// 최대 패턴 난이도 반환
        /// </summary>
        public float GetMaxDifficulty()
        {
            return Mathf.Max(trill, stairs, chord, denim, jacks, longNoteHybrid, burst, offbeat);
        }

        /// <summary>
        /// .synth 파일 형식으로 출력
        /// </summary>
        public string ToSynthFormat()
        {
            string result = "[PATTERN_DIFFICULTY]\n";
            result += $"Trill: {trill:F1}\n";
            result += $"Stairs: {stairs:F1}\n";
            result += $"Chord: {chord:F1}\n";
            result += $"Denim: {denim:F1}\n";
            result += $"Jacks: {jacks:F1}\n";
            result += $"LongNoteHybrid: {longNoteHybrid:F1}\n";
            result += $"Burst: {burst:F1}\n";
            result += $"Offbeat: {offbeat:F1}\n";
            return result;
        }

        /// <summary>
        /// .synth 형식에서 파싱
        /// </summary>
        public static PatternDifficulty ParseFromSynthFormat(string[] lines)
        {
            PatternDifficulty pd = new PatternDifficulty();

            foreach (string line in lines)
            {
                if (line.StartsWith("Trill:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.trill);
                else if (line.StartsWith("Stairs:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.stairs);
                else if (line.StartsWith("Chord:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.chord);
                else if (line.StartsWith("Denim:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.denim);
                else if (line.StartsWith("Jacks:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.jacks);
                else if (line.StartsWith("LongNoteHybrid:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.longNoteHybrid);
                else if (line.StartsWith("Burst:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.burst);
                else if (line.StartsWith("Offbeat:"))
                    float.TryParse(line.Split(':')[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out pd.offbeat);
            }

            return pd;
        }
    }
}
