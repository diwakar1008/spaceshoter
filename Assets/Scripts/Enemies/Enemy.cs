using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A class which controls enemy behaviour
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("The speed at which the enemy moves.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The score value for defeating this enemy")]
    public int scoreValue = 5;

    [Header("Following Settings")]
    [Tooltip("The transform of the object that this enemy should follow.")]
    public Transform followTarget = null;
    [Tooltip("The distance at which the enemy begins following the follow target.")]
    public float followRange = 10.0f;

    [Header("Shooting")]
    [Tooltip("The enemy's gun components")]
    public List<ShootingController> guns = new List<ShootingController>();

    /// <summary>
    /// Enum to help with shooting modes
    /// </summary>
    public enum ShootMode { None, ShootAll };

    [Tooltip("The way the enemy shoots:\n" +
        "None: Enemy does not shoot.\n" +
        "ShootAll: Enemy fires all guns whenever it can.")]
    public ShootMode shootMode = ShootMode.ShootAll;

    /// <summary>
    /// Enum to help wih different movement modes
    /// </summary>
    public enum MovementModes { NoMovement, FollowTarget, Scroll };

    [Tooltip("The way this enemy will move\n" +
        "NoMovement: This enemy will not move.\n" +
        "FollowTarget: This enemy will follow the assigned target.\n" +
        "Scroll: This enemy will move in one horizontal direction only.")]
    public MovementModes movementMode = MovementModes.FollowTarget;

    //The direction that this enemy will try to scroll if it is set as a scrolling enemy.
    [SerializeField] private Vector3 scrollDirection = Vector3.right;

    private void LateUpdate()
    {
        HandleBehaviour();       
    }

    private void Start()
    {
        if (followTarget == null)
        {
            if (GameManager.instance != null && GameManager.instance.player != null)
            {
                followTarget = GameManager.instance.player.transform;
            }
        }
        if (movementMode == MovementModes.Scroll)
        {
            originalPosition = transform.position;
            turnPosition = originalPosition + scrollDirection;
        }
    }

    
    private void HandleBehaviour()
    {
        MoveEnemy();
        // Attempt to shoot, according to this enemy's shooting mode
        TryToShoot();
    }

    
    public void DoBeforeDestroy()
    {
        AddToScore();
        IncrementEnemiesDefeated();
    }

    
    private void AddToScore()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.AddScore(scoreValue);
        }
    }

    
    private void IncrementEnemiesDefeated()
    {
        if (GameManager.instance != null && !GameManager.instance.gameIsOver)
        {
            GameManager.instance.IncrementEnemiesDefeated();
        }       
    }

    
    private void MoveEnemy()
    {
        // Determine correct movement
        Vector3 movement = GetDesiredMovement();

        // Determine correct rotation
        Quaternion rotationToTarget = GetDesiredRotation();

        // Move and rotate the enemy
        transform.position = transform.position + movement;
        transform.rotation = rotationToTarget;
    }

    
    protected virtual Vector3 GetDesiredMovement()
    {
        Vector3 movement;
        switch(movementMode)
        {
            case MovementModes.FollowTarget:
                movement = GetFollowPlayerMovement();
                break;
            case MovementModes.Scroll:
                movement = GetScrollingMovement();
                break;
            default:
                movement = Vector3.zero;
                break;
        }
        return movement;
    }

    
    protected virtual Quaternion GetDesiredRotation()
    {
        Quaternion rotation;
        switch (movementMode)
        {
            case MovementModes.FollowTarget:
                rotation = GetFollowPlayerRotation();
                break;
            case MovementModes.Scroll:
                rotation = GetScrollingRotation();
                break;
            default:
                rotation = transform.rotation;
                break;
        }
        return rotation;
    }

    private void TryToShoot()
    {
        switch (shootMode)
        {
            case ShootMode.None:
                break;
            case ShootMode.ShootAll:
                foreach (ShootingController gun in guns)
                {
                    gun.Fire();
                }
                break;
        }
    }

    private Vector3 GetFollowPlayerMovement()
    {
        // Check if the target is in range, then move
        if (followTarget != null && (followTarget.position - transform.position).magnitude < followRange)
        {
            Vector3 moveDirection = (followTarget.position - transform.position).normalized;
            Vector3 movement = moveDirection * moveSpeed * Time.deltaTime;
            return movement;
        }
        return Vector3.zero;
    }

  
    private Quaternion GetFollowPlayerRotation()
    {
        if (followTarget == null)
        {
            return transform.rotation;
        }
        float angle = Vector3.SignedAngle(Vector3.down, (followTarget.position - transform.position).normalized, Vector3.forward);
        Quaternion rotationToTarget = Quaternion.Euler(0, 0, angle);
        return rotationToTarget;
    }

    
    private Vector3 GetScrollingMovement()
    {
        scrollDirection = GetScrollDirection();
        Vector3 movement = scrollDirection.normalized * moveSpeed * Time.deltaTime;
        return movement;
    }

    
    private Quaternion GetScrollingRotation()
    {
        return Quaternion.identity;
    }

    private Vector3 originalPosition;
    private Vector3 turnPosition;
    
    private Vector3 GetScrollDirection()
    {
        bool overX = false;
        bool overY = false;
        bool overZ = false;

        Vector3 directionFromCurrentPositionToTarget = turnPosition - transform.position;

        if ((directionFromCurrentPositionToTarget.x <= 0.0001 && directionFromCurrentPositionToTarget.x >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.x) != Mathf.Sign(scrollDirection.x))
        {
            overX = true;
            transform.position = new Vector3(turnPosition.x, transform.position.y, transform.position.z);
        }
        if ((directionFromCurrentPositionToTarget.y <= 0.0001 && directionFromCurrentPositionToTarget.y >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.y) != Mathf.Sign(scrollDirection.y))
        {
            overY = true;
            transform.position = new Vector3(transform.position.x, turnPosition.y, transform.position.z);
        }
        if ((directionFromCurrentPositionToTarget.z <= 0.0001 && directionFromCurrentPositionToTarget.z >= -0.0001) || Mathf.Sign(directionFromCurrentPositionToTarget.z) != Mathf.Sign(scrollDirection.z))
        {
            overZ = true;
            transform.position = new Vector3(transform.position.x, transform.position.y, turnPosition.z);
        }

        if (overX && overY && overZ)
        {
            turnPosition = originalPosition - scrollDirection;
            return scrollDirection * -1;
        }
        return scrollDirection;
    }
}
