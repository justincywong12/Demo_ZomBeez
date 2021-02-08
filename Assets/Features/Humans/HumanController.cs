﻿using UnityEngine;

public enum Pathfinding { None, Astar };

public class HumanController : NavAgent
{
    [SerializeField]
    [Header("Settings")]
    private HumanSettings _settings = null;
    [SerializeField]
    private GameObject _modelHolder = null;
    
    private HumanBrain _brain;
    private HumanAnimController _animController;
    private HumanInventory _inventory;
    private Pathfinding _currentPathing = Pathfinding.None;
    private Transform target;
    private float _currentSpeed = 0;
    //private float _currentBrains;
    private bool _isDead = false;
    private bool _isAware = false;

    //public float CurrentBrains => _currentBrains;
    public HumanInventory Inventory => _inventory;
    public Pathfinding CurrentPathing => _currentPathing;
    public HumanSettings Settings {get {return _settings;}}
    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }
    
    public bool HasJelly => _hasJelly;
    public bool _hasJelly = false;

    private void Update()
    {
        Pathing();

        MovePlayer();
    }

    public void Initialise(HumanSO so)
    {
        //Instantiate model
        GameObject go = Instantiate(so.model, _modelHolder.transform);
        
        //References
        GetComponent<Droppable>().Initialise();
        _animController = GetComponent<HumanAnimController>();
        _animController.Initialise();
        _inventory = GetComponent<HumanInventory>();
        _brain = GetComponent<HumanBrain>();
        _brain.Initialise(so);
    }

    void Pathing()
    {
        //Auto random pathing
        switch (CurrentPathing)
        {
            case Pathfinding.None:
                break;
            case Pathfinding.Astar:
                if(AtDestination())
                {
                    currentPath = AStarSearch(currentPath[currentPathIndex], FindClosestWaypoint(target));
                    currentPathIndex = 0;
                }
                break;
        }
    }

    public int FindClosestWaypoint(Transform target)
    {
        if (!target)
        {
            Debug.Log("No target transform");
            return -1;
        }

        //Find which waypoint is closest to target
        float distance = Mathf.Infinity;
        int closestWaypoint = -1;

        //Find closest node to target
        for (int i = 0; i < graphNodes.graphNodes.Length; i++)
        {
            if (Vector3.Distance(target.transform.position, graphNodes.graphNodes[i].transform.position) <= distance)
            {
                distance = Vector3.Distance(target.transform.position, graphNodes.graphNodes[i].transform.position);
                closestWaypoint = i;
            }
        }

        return closestWaypoint;
    }

    public Transform HiveTrans()
    {
        return FindObjectOfType<Hive>().transform;
    }

    public Transform DepotTrans()
    {
        return FindObjectOfType<Depot>().transform;
    }

    public Transform ClosestActiveForageSite()
    {
        //Store all forage sites
         ForageSite [] allSites = FindObjectsOfType<ForageSite>();

         //Find which active, forage site is closest 
         ForageSite closestSite = null;
         float closestDist = Mathf.Infinity;
         
         foreach (ForageSite site in allSites)
         {
             float tempDist = Vector3.Distance(transform.position, site.transform.position);
             
             if (tempDist < closestDist && 
                 site.currentItem)
             {
                 closestSite = site;
                 closestDist = tempDist;
             }
         }

         return closestSite.transform;
    }

    public bool AtDestination()
    {
        if (currentPath.Count > 0)
        {
            bool value = currentNodeIndex == currentPath[currentPath.Count - 1];
            return value;
        }
        else
        {
            return false;
        }
    }

    void MovePlayer()
    {
        //Move player
        if (currentPath.Count > 0 && !_isDead && !GetComponent<Droppable>())
        {
            var targetPos = graphNodes.graphNodes[currentPath[currentPathIndex]].transform.position;

            //Set speed
            if (_currentSpeed < Settings.Speed)
            {
                _currentSpeed += Settings.Speed * Time.deltaTime * Settings.Acceleration;
            }

            //Move towards next node
            Transform transform1;
            (transform1 = transform).position = Vector3.MoveTowards(transform.position, targetPos, _currentSpeed * Time.deltaTime);

            //Face direction
            var targetDir = targetPos - transform1.position;
            var step = _currentSpeed * Time.deltaTime;
            var newDir = Vector3.RotateTowards(transform1.forward, targetDir, step, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);

            //Inc path index
            if (Vector3.Distance(transform.position, graphNodes.graphNodes[currentPath[currentPathIndex]].transform.position) <= Settings.MinDistance)
            {
                if (currentPathIndex < currentPath.Count - 1)
                {
                    currentPathIndex++;
                }
            }

            //Store current node index
            currentNodeIndex = graphNodes.graphNodes[currentPath[currentPathIndex]].GetComponent<LinkedNode>().index;
        }
    
    
        //Set speed
        if (_currentSpeed > 0 && CurrentPathing == Pathfinding.None)
        {
            _currentSpeed -= Settings.Speed * Time.deltaTime * Settings.Deceleration;
        }

        //Update anim variables
        _animController.Float_VelocityZ(_currentSpeed);
    }

    public void LoseBrains(int val)
    {
        _settings.ChangeBrains(-val);

        if (_settings.Brains <= 0)
        {
            _isDead = true;
            _animController.Bool_IsDead(true);
            Destroy(this.gameObject, 5.0f);
        }
    }

    public void TakeDamage(float dmg)
    {
    //Armour calc
        float val = Settings.Armour - dmg;
        if (val >= 0)
        {
            Settings.SetArmour(val);
            return;
        }
        else if (val < 0)
        {
            Settings.SetArmour(0);
            
    //Health calc
        Settings.SetHealth(Settings.Health + val);
            
    //Death logic
            if (Settings.Health <= 0)
            {
                _isDead = true;
                _animController.Bool_IsDead(true);
                Destroy(this.gameObject, 5.0f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Aware of bees?
        if (!_isAware)
        {
            _isAware = other.GetComponent<Worker>();
        }
        
        //Jelly?
        if (other.gameObject.GetComponent<Hive>())
        {
            other.gameObject.GetComponent<Hive>().LoseJelly();
            _hasJelly = true;
        }
    }

    public bool IsAware() => _isAware;

    public void SetPathFinding(Pathfinding val) => _currentPathing = val;

    public bool HealthCheck(float percentage)
    {
        return (Settings.Health / 100) <= percentage;
    }
}

