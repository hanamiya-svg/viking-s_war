using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

namespace UnityRoyale
{
    public class CardManager : MonoBehaviour
    {
        public Camera mainCamera;
        public LayerMask playingFieldMask;
        public GameObject cardPrefab;
        public DeckData playersDeck;
        public MeshRenderer forbiddenAreaRenderer;

        public UnityAction<CardData, Vector3, Placeable.Faction> OnCardUsed;

        [Header("UI Elements")]
        public RectTransform cardsDashboard;
        public RectTransform cardsPanel;

        private Card[] cards;
        private bool cardIsActive = false;
        private GameObject previewHolder;
        private Vector3 inputCreationOffset = new Vector3(0f, 0f, 1f);

        private const float INITIAL_DRAW_DELAY = 0.8f;
        private const float REPLACE_DELAY = 3f;

        private void Awake()
        {
            previewHolder = new GameObject("PreviewHolder");
            cards = new Card[3];
        }

        public void LoadDeck()
        {
            DeckLoader loader = gameObject.AddComponent<DeckLoader>();
            loader.OnDeckLoaded += DeckLoaded;
            loader.LoadDeck(playersDeck);
        }

        private void DeckLoaded()
        {
            Debug.Log("Player's deck loaded");
            StartCoroutine(InitialDeal());
        }

        /* =======================
           CARD CREATION / DEALING
           ======================= */

        private IEnumerator InitialDeal()
        {
            RectTransform createdCard = null;

            yield return AddCardToDeck(INITIAL_DRAW_DELAY, c => createdCard = c);

            for (int i = 0; i < cards.Length; i++)
            {
                yield return PromoteCard(createdCard, i, 0.4f);
                yield return AddCardToDeck(INITIAL_DRAW_DELAY, c => createdCard = c);
            }
        }

        private IEnumerator ReplaceUsedCard(int position)
        {
            RectTransform createdCard = null;

            yield return AddCardToDeck(REPLACE_DELAY, c => createdCard = c);
            yield return PromoteCard(createdCard, position, 0.2f);
        }

        private IEnumerator AddCardToDeck(float delay, Action<RectTransform> onCreated)
        {
            yield return new WaitForSeconds(delay);

            RectTransform cardTransform =
                Instantiate(cardPrefab, cardsPanel).GetComponent<RectTransform>();

            cardTransform.localScale = Vector3.one * 0.7f;
            cardTransform.anchoredPosition = new Vector2(180f, -300f);

            cardTransform
                .DOAnchorPos(new Vector2(180f, 0f), 0.25f)
                .SetEase(Ease.OutQuad);

            Card card = cardTransform.GetComponent<Card>();
            card.InitialiseWithData(playersDeck.GetNextCardFromDeck());

            onCreated?.Invoke(cardTransform);
        }

        private IEnumerator PromoteCard(RectTransform cardTransform, int position, float delay)
        {
            yield return new WaitForSeconds(delay);

            cardTransform.SetParent(cardsDashboard, true);

            cardTransform
                .DOAnchorPos(new Vector2(210f * (position + 1) + 20f, 0f), 0.25f)
                .SetEase(Ease.OutQuad);

            cardTransform.localScale = Vector3.one;

            Card card = cardTransform.GetComponent<Card>();
            card.cardId = position;
            cards[position] = card;

            card.OnTapDownAction += CardTapped;
            card.OnDragAction += CardDragged;
            card.OnTapReleaseAction += CardReleased;
        }

        /* =======================
           CARD INTERACTION
           ======================= */

        private void CardTapped(int cardId)
        {
            cards[cardId].GetComponent<RectTransform>().SetAsLastSibling();
            forbiddenAreaRenderer.enabled = true;
        }

        private void CardDragged(int cardId, Vector2 dragAmount)
        {
            cards[cardId].transform.Translate(dragAmount);

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            bool hitPlane = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playingFieldMask);

            if (hitPlane)
            {
                if (!cardIsActive)
                {
                    cardIsActive = true;
                    previewHolder.transform.position = hit.point;
                    cards[cardId].ChangeActiveState(true);

                    PlaceableData[] data = cards[cardId].cardData.placeablesData;
                    Vector3[] offsets = cards[cardId].cardData.relativeOffsets;

                    for (int i = 0; i < data.Length; i++)
                    {
                        Instantiate(
                            data[i].associatedPrefab,
                            hit.point + offsets[i] + inputCreationOffset,
                            Quaternion.identity,
                            previewHolder.transform
                        );
                    }
                }
                else
                {
                    previewHolder.transform.position = hit.point;
                }
            }
            else if (cardIsActive)
            {
                cardIsActive = false;
                cards[cardId].ChangeActiveState(false);
                ClearPreviewObjects();
            }
        }

        private void CardReleased(int cardId)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playingFieldMask))
            {
                OnCardUsed?.Invoke(
                    cards[cardId].cardData,
                    hit.point + inputCreationOffset,
                    Placeable.Faction.Player
                );

                ClearPreviewObjects();
                Destroy(cards[cardId].gameObject);

                StartCoroutine(ReplaceUsedCard(cardId));
            }
            else
            {
                cards[cardId].GetComponent<RectTransform>()
                    .DOAnchorPos(new Vector2(220f * (cardId + 1), 0f), 0.25f)
                    .SetEase(Ease.OutQuad);
            }

            forbiddenAreaRenderer.enabled = false;
        }

        private void ClearPreviewObjects()
        {
            for (int i = previewHolder.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(previewHolder.transform.GetChild(i).gameObject);
            }
        }
    }
}
