﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Snake : MonoBehaviour
{
    public GameObject block;
    public GameObject itemPrefab;
    public GameObject scenary;
    public Text pointsText;
    public int width, height;

    public float movementInterval;
    public float explosionForce;

    private bool gameRunning = true;

    private int points;
    private GameObject item;
    private GameObject head;
    private Queue<GameObject> body = new Queue<GameObject>();
    private Vector3 direction = Vector3.right;

    private enum SquareState
    {
        FREE, BLOCKED, ITEM
    }


    private SquareState[,] map;

    private void Awake()
    {
        points = 0;
        BuildScenary();
        head = NewBlock(width/2f, height/2f);

        GenerateRandomItem();
        StartCoroutine(Movement());
    }

    private void GenerateRandomItem()
    {
        Vector3 position = GetFreeSquare();
        if(item != null)
        {
            item.transform.position = GetFreeSquare();
            SetSquareState(item.transform.position, SquareState.ITEM);
        } else
        {
            item = NewItem(position.x, position.y);
        }
    }

    private Vector3 GetFreeSquare()
    {
        List<Vector3> freeSquares = new List<Vector3>();

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(Mathf.RoundToInt(x), Mathf.RoundToInt(y));

                if(GetSquareState(position) == SquareState.FREE)
                {
                    freeSquares.Add(position);
                }
            }
        }

        return freeSquares[Random.Range(0, freeSquares.Count - 1)];
    }

    private GameObject NewItem(float x, float y)
    {
        Vector3 position = new Vector3(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        GameObject instance = Instantiate(itemPrefab, position, Quaternion.identity, scenary.transform);
        
        SetSquareState(position, SquareState.ITEM);

        return instance;
    }

    private GameObject NewBlock(float x, float y)
    {
        Vector3 position = new Vector3(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        GameObject instance = Instantiate(block, position, Quaternion.identity, this.transform);

        body.Enqueue(instance);
        SetSquareState(position, SquareState.BLOCKED);

        return instance;
    }

    private IEnumerator Movement()
    {
        WaitForSeconds wait = new WaitForSeconds(movementInterval);

        while (gameRunning)
        {
            Vector3 nextPosition = head.transform.position + direction;

            if(GetSquareState(nextPosition) == SquareState.BLOCKED)
            {
                GameOver();
                yield return new WaitForSeconds(5f);

            } else
            {
                if (GetSquareState(nextPosition) == SquareState.ITEM)
                {
                    GameObject newHead = NewBlock(Mathf.RoundToInt(nextPosition.x), Mathf.RoundToInt(nextPosition.y));
                    AddPoint();
                    GenerateRandomItem();
                    head = newHead;
                } else
                {
                    GameObject tail = body.Dequeue();
                    SetSquareState(tail.transform.position, SquareState.FREE);
                    tail.transform.position = nextPosition;
                    SetSquareState(tail.transform.position, SquareState.BLOCKED);
                    body.Enqueue(tail);

                    head = tail;
                }

                yield return wait;
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void GameOver()
    {
        gameRunning = false;

        Explode(scenary.GetComponentsInChildren<Rigidbody>());
        Explode(this.GetComponentsInChildren<Rigidbody>());
    }

    private void Explode(Rigidbody[] rbs_blocks)
    {
        foreach(Rigidbody rb in rbs_blocks)
        {
            rb.useGravity = true;
            rb.AddForce(Random.insideUnitCircle.normalized * explosionForce);
            rb.AddTorque(0, 0, Random.Range(-explosionForce, explosionForce));
        }
    }

    private void AddPoint()
    {
        points++;
        pointsText.text = points.ToString();
    }

    private void BuildScenary()
    {
        map = new SquareState[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x, y);
                if ((x == 0) || (y == 0) || (x == width-1) || (y == height - 1))
                {
                    GameObject instance = Instantiate(block, position, Quaternion.identity, scenary.transform);
                    SetSquareState(position, SquareState.BLOCKED);
                } else
                {
                    SetSquareState(position, SquareState.FREE);
                }
            }
        }
    }

    private SquareState GetSquareState(Vector3 position)
    {
        return map[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)];
    }

    private void SetSquareState(Vector3 position, SquareState state)
    {
        map[Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)] = state;
    }

    public void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontal, vertical);

        if(inputDirection != Vector3.zero)
        {
            if(Mathf.Abs(inputDirection.magnitude) > 1)
            {
                inputDirection.y = 0;
            }

            direction = inputDirection;
        }
    }
}
