🎮 ActionFit Code Test - Unity Client Developer

📁 구현 파일 구조 및 역할 설명

```
Assets/Project/Scenes
├── StageDataTestScene.cs                // StageEditor 테스트 및 미리보기용 씬 구성

Assets/Project/Scripts/Controller
├── BoardController.cs                   // 전체 스테이지 흐름 및 로직 컨트롤러
├── BoardController+StageData.cs         // StageData 로딩/초기화 전용 분리 스크립트
├── BlockDestroyHelper.cs                // 블록 파괴 트리거 및 이벤트 도우미

Assets/Project/Scripts/Controller/MVC
├── BlockFactory.cs                      // BoardBlock 생성 및 Addressable 로딩 처리
├── WallFactory.cs                       // WallData 기반 Wall 객체 생성
├── PlayingBlockSpawner.cs               // StageData 기반 플레이블록 생성
├── BoardMaskRenderer.cs                 // 외곽 마스킹 Quad 처리 (Stencil 전환 시도 포함)
├── ParticleEffectController.cs          // 블록 파괴 이펙트 및 이펙트 풀링 처리

Assets/Project/Scripts/Handler
├── BlockDragHandler.cs                  // Drag&Drop 입력 처리 및 이벤트 분배 핵심 핸들러

Assets/Project/Scripts/Handler/MVC
├── BlockMouseInput.cs                   // 실제 마우스 입력 감지 처리
├── BlockCollisionState.cs               // 블록의 충돌 상태 유지 및 이벤트 분리
├── BlockMovementController.cs           // 블록 이동 상태 제어 및 물리 반응 처리
├── BlockOutlineController.cs            // 선택된 블록의 외곽선 연출 및 피드백 처리

Assets/Project/Scripts/Util
├── StagePreviewBridge.cs                // 에디터에서 만든 StageData를 플레이모드로 전달하는 브릿지 (DontDestroyOnLoad 활용)

Assets/Editor
├── StageDataEditor.cs                   // ScriptableObject 기반 StageData 생성 및 편집 기능 제공
```

🧩 구현 내용 요약

01. 🛠️ 코드 리팩토링

✅ 주요 개선 사항

MVC 패턴 기반 구조화

BoardController의 기능을 세분화: Factory, Spawner 등으로 역할 분리

입력 / 상태 / 렌더링 역할 분리

BlockDragHandler 분리

드래그 처리와 물리 로직을 별도 분리하여 SRP 원칙 적용

02. 🧩 Stage Editor 구현

✅ 구현 방식

EditorWindow 기반 커스텀 에디터 구현

ScriptableObject 기반 StageData 생성 및 저장 기능 지원

✨ 기능 목록

NxN 크기의 BoardBlock 자동 생성

PlayingBlock(Shape, Gimmick 포함) 구성

Wall 및 출구 위치 설정

Editor Play 버튼 클릭 시 테스트 씬에서 Stage 미리보기 실행

Resources/StageData에 .asset으로 저장되며 실행 시 불러옴

03. ✨ Visual Effect 최적화

⚠️ 구현 실패 (기술적 제약으로 보류)

목표: 기존 Quad 기반 가림 처리 → Stencil Buffer 기반 Vertex 셰이더 방식 전환

진행 내용:

Stencil Mask Shader 작성 및 단일 Quad로 Replace 처리 완료

보드 내부만 보여주기 위한 StencilVisible 셰이더 (Comp Equal / Pass Keep) 적용 시도 → 렌더 순서 문제 및 URP Feature 미연동 문제로 실패

🛠️ 시도한 구조

RenderObject 2개 구성 (Mask용 Quad: Replace / 나머지: Keep 비교) → Mask 처리 의도대로 작동하지 않음

Material에 Stencil 옵션 적용 시 정상 렌더링 불가 (머티리얼 미리보기, 객체 렌더링 문제)

ShaderGraph, 커스텀 쉐이더 다수 시도 → URP Renderer Feature 설정 부재로 인한 적용 실패

🔧 Addressables 적용

기존 Resources.Load 방식 → Addressables 기반 비동기 로딩 전환

적용 프리팹: Board Cell.prefab, Quad.prefab

로딩 시 한 번만 불러온 후 Dictionary<string, GameObject>로 캐싱하여 재사용

