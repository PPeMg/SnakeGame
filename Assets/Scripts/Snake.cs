using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    public GameObject block;
    public GameObject item;
    public GameObject scenary;
    public int width, height;

    public float movementInterval;

    private bool gameRunning = true;

    private GameObject head;
    private Queue<GameObject> body = new Queue<GameObject>();
    private Vector3 direction = Vector3.right;

    private enum SquareState
    {
        FREE, BLOCKED
    }


    private SquareState[,] map;

    private void Awake()
    {
        BuildScenary();
        head = NewBlock(width/2f, height/2f);

        StartCoroutine(Movement());
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
                Debug.Log("DEAD");
                yield return new WaitForSeconds(5f);
                gameRunning = false;

            } else
            {
                GameObject tail = body.Dequeue();
                SetSquareState(tail.transform.position, SquareState.FREE);
                tail.transform.position = nextPosition;
                SetSquareState(tail.transform.position, SquareState.BLOCKED);
                body.Enqueue(tail);

                head = tail;

                yield return wait;
            }
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
