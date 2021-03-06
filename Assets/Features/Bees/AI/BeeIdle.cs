﻿using UnityEngine;

public class BeeIdle : BeeAIStates
{
    public BeeIdle(BeeController bee) : base(bee)
    {
    }
    
    public override void Initialise()
    {
        bee.Target = hive.gameObject;
    }
    
    public override void Tick()
    {
        bee.OrbitPos(bee.Target.transform.position);
    }
}
