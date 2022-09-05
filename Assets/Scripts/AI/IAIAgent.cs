using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAIAgent
{
    // Define agent's action space
    void AIAgentMoveDir(int dir);
    void AIAgentDoAction(int actionNum);

    int AIAgentGetMovementLeft();
}
