﻿using UnityEngine;
namespace Lockstep {
public class MovementGroup
{       
    const int MinGroupSize = 3;

    public Vector2d Destination { get; private set; }

    FastList<Move> movers;
    Vector2d groupDirection;
    public Vector2d groupPosition;

    public int indexID { get; set; }

    int moversCount;
    long radius;
    long averageCollisionSize;
    bool calculatedBehaviors;

    public void Initialize(Command com)
    {
        Destination = com.Position;
        calculatedBehaviors = false;
        movers = new FastList<Move>(com.Select.selectedAgentLocalIDs.Count);
    }

    public void Add(Move mover)
    {
        if (mover.MyMovementGroup .IsNotNull())
        {
            mover.MyMovementGroup.movers.Remove(mover);
        }
        mover.MyMovementGroup = this;
        mover.MyMovementGroupID = indexID;
        movers.Add(mover);
        moversCount++;
    }

    public void Remove(Move mover)
    {
        moversCount--;
    }

    public void LocalSimulate()
    {

    }
    public void LateSimulate () {
        if (!calculatedBehaviors)
        {
            CalculateAndExecuteBehaviors();
            calculatedBehaviors = true;
        }
        if (movers.Count == 0)
        {
            Deactivate();
        }
    }

    public MovementType movementType { get; private set; }

    public void CalculateAndExecuteBehaviors()
    {

        Move mover;

        if (movers.Count >= MinGroupSize)
        {
            averageCollisionSize = 0;
            groupPosition = Vector2d.zero;
            for (int i = 0; i < movers.Count; i++)
            {
                mover = movers [i];
                groupPosition += mover.Position;
                averageCollisionSize += mover.CollisionSize;
            }

            groupPosition /= movers.Count;
            averageCollisionSize /= movers.Count;

            long biggestSqrDistance = 0;
            for (int i = 0; i < movers.Count; i++)
            {
                long currentSqrDistance = movers [i].Position.SqrDistance(groupPosition.x, groupPosition.y);
                if (currentSqrDistance > biggestSqrDistance)
                {
                    long currentDistance = FixedMath.Sqrt(currentSqrDistance);
                    /*
                    DistDif = currentDistance - Radius;
                    if (DistDif > MaximumDistDif * MoversCount / 128) {
                        ExecuteGroupIndividualMove ();
                        return;
                    }*/
                    biggestSqrDistance = currentSqrDistance;
                    radius = currentDistance;
                }
            }
            if (radius == 0)
            {
                ExecuteGroupIndividualMove();
                return;
            }
            long expectedSize = averageCollisionSize.Mul(averageCollisionSize).Mul(FixedMath.One * 2).Mul(movers.Count);
            long groupSize = radius.Mul(radius);

            if (groupSize > expectedSize || groupPosition.FastDistance(Destination.x, Destination.y) < (radius * radius))
            {
                ExecuteGroupIndividualMove();
                return;
            }
            ExecuteGroupMove ();

        } else
        {
            ExecuteIndividualMove();
        }
    }

    public void Deactivate()
    {
        Move mover;
        for (int i = 0; i < movers.Count; i++)
        {
            mover = movers [i];
            mover.MyMovementGroup = null;
            mover.MyMovementGroupID = -1;
        }
        movers.FastClear();
        MovementGroupHelper.Pool(this);
        calculatedBehaviors = false;
        indexID = -1;
    }
    void ExecuteGroupMove () {
        movementType = MovementType.Group;
        groupDirection = Destination - groupPosition;

        for (int i = 0; i < movers.Count; i++)
        {
            Move mover = movers [i];
            mover.IsFormationMoving = true;
            mover.CollisionStopMultiplier = Move.FormationStop;
            mover.OnGroupProcessed(mover.Position + groupDirection);
        }
    }
    void ExecuteIndividualMove () {
        movementType = MovementType.Individual;
        for (int i = 0; i < movers.Count; i++)
        {
            Move mover = movers [i];
            mover.IsFormationMoving = false;
            mover.CollisionStopMultiplier = Move.DirectStop;
            mover.OnGroupProcessed(Destination);
        }
    }
    void ExecuteGroupIndividualMove()
    {
        movementType = MovementType.GroupIndividual;
        for (int i = 0; i < movers.Count; i++)
        {
            Move mover = movers [i];
            mover.IsFormationMoving = false;
            mover.CollisionStopMultiplier = Move.GroupDirectStop;
            mover.OnGroupProcessed(Destination);
        }
    }
}

public enum MovementType : long
{
    Group,
    GroupIndividual,
    Individual
}
}
