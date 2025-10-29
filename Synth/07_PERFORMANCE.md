# 성능 최적화 (Performance Optimization)

> **최근 업데이트**: 2025-10-27
> **우선순위**: 중간

[← 메인 TODO로 돌아가기](DEVELOPMENT_TODO.md)

---

## ⚡ 성능 최적화 (Performance)

### 1. 오브젝트 풀링 개선
**현재 상태**: `NoteManager.cs`에서 100개 노트 풀링

**개선 사항**:
```
[ ] 동적 풀 크기 조정
    [ ] 차트 노트 수에 따라 자동 조정
    [ ] 최소 100개, 최대 500개

[ ] 롱노트 별도 풀
    [ ] 일반 노트와 분리
    [ ] 필요 시에만 생성

[ ] 이펙트 오브젝트 풀링
    [ ] 판정 이펙트
    [ ] 파티클 시스템
```

---

### 2. 머티리얼 캐싱
```
[ ] MaterialPropertyBlock 사용
    [ ] 노트 색상 변경 시 머티리얼 인스턴스 생성 방지

[ ] 텍스처 아틀라스
    [ ] UI 스프라이트 통합
    [ ] 드로우 콜 감소
```

---

### 3. 코루틴 최적화
```
[ ] WaitForSeconds 캐싱
    private WaitForSeconds waitTime = new WaitForSeconds(0.1f);

[ ] 불필요한 코루틴 제거
    [ ] 매 프레임 Update로 대체 가능한 부분 확인
```

---

### 4. Primitive 생성 제거
**파일**: `RhythmManager.cs`, `GearController.cs`

```
[ ] GC 압박 감소
    - new Vector3() → Vector3.zero
    - new Color() → Color.white

[ ] StringBuilder 사용
    - 문자열 연결 최적화
```

---

### 5. 업데이트 최적화
```
[ ] FixedUpdate와 Update 분리
    [ ] 물리 관련 → FixedUpdate
    [ ] UI 업데이트 → Update

[ ] 프레임 스킵 고려
    [ ] 저사양 모드 구현
```
