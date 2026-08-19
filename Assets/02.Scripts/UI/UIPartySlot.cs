using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

namespace AFKHero.UI
{
    public class UIPartySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private int slotIndex;             //파티 슬롯 번호
        [SerializeField] private Transform heroPrefabPos;   //영웅 프리펩이 생성되는 위치
        [SerializeField] private RectTransform dragArea;    //드래그 중 영웅이 올라갈 영역

        private GameObject dragHeroPrefab;                  //현재 드래그 중인 영웅 프리펩
        private bool isDrag;                                //드래그 여부


        //드래그 시작
        public void OnBeginDrag(PointerEventData eventData)
        {
            HeroInstance hero = PartyManager.Instance.partySlots[slotIndex];

            if (hero == null || hero.data == null) return; //빈자리면 드래그 X
            if (heroPrefabPos.childCount == 0) return;     //영웅 프리펩이 없으면 드래그 X
            if (dragArea == null) return;

            dragHeroPrefab = heroPrefabPos.GetChild(0).gameObject; //현재 슬롯의 영웅 프리펩 (heroPrefabPos아래에 영웅프리팹이 생성되므로 GetChild(0))
            isDrag = true;

            dragHeroPrefab.transform.SetParent(dragArea, true); //현재 위치를 유지하면서 드래그 영역으로 이동

            //드래그 중 다른 영웅보다 위에 표시
            SortingGroup sortingGroup = dragHeroPrefab.GetComponentInChildren<SortingGroup>();

            if (sortingGroup != null)
            {
                sortingGroup.sortingLayerName = "UI";
                sortingGroup.sortingOrder = 30;
            }

            MoveHero(eventData);
        }

        //드래그 중
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDrag) return;

            MoveHero(eventData);
        }

        //드래그 중 영웅 이동
        private void MoveHero(PointerEventData eventData)
        {
            if (dragHeroPrefab == null) return;

            //마우스 위치를 dragArea 기준의 월드 좌표로 변환
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragArea, eventData.position, eventData.pressEventCamera, out Vector3 worldPosition))
            {
                dragHeroPrefab.transform.position = worldPosition; //영웅 프리펩을 마우스 위치로 이동
            }
        }

        //다른 파티 슬롯에 드롭
        public void OnDrop(PointerEventData eventData)
        {
            UIPartySlot dragSlot = eventData.pointerDrag.GetComponent<UIPartySlot>(); //드래그를 시작한 슬롯 가져오기

            if (dragSlot == null) return;
            if (dragSlot.slotIndex == slotIndex) return;

            SwapPartySlot(dragSlot.slotIndex, slotIndex); //드래그한 슬롯과 현재 슬롯 스왑
        }

        //드래그 종료
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDrag) return;

            dragHeroPrefab = null;
            isDrag = false;

            UIPartyManager.Instance.UpdatePartyUI(); //변경된 파티 UI 갱신
        }

        //파티 슬롯 스왑
        private void SwapPartySlot(int fromIndex, int toIndex)
        {
            HeroInstance[] partySlots = PartyManager.Instance.partySlots;

            HeroInstance tempHero = partySlots[fromIndex];
            partySlots[fromIndex] = partySlots[toIndex];
            partySlots[toIndex] = tempHero;
        }
    }
}