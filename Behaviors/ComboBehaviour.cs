using UnityEngine;
using System.Collections;
using UnitySteer;
using UnitySteer.Vehicles;

public class ComboBehaviour : VehicleBehaviour, IRadarReceiver
{
	public float maxSpeed, maxForce;
	public LayerMask obstacleLayers;
	public float steerToAvoidNeighbors, steerToAvoidObstacles, steerToStayOnPath, steerForTargetSpeed;
	private float steerForPursuit;
	
	private ComboVehicle vehicle;
	private Vehicle target;
    private static Hashtable obstacles;
		// TODO: Need a better system for this
	
	
	
	public void Awake()
	{
		if( rigidbody == null )
		{
			vehicle = new ComboVehicle( transform, 1.0f );
		}
		else
		{
			vehicle = new ComboVehicle( rigidbody );
		}
		
		if( obstacles == null )
		{
			obstacles = new Hashtable();	
		}
		
		MaxSpeed = maxSpeed;
		SteerToAvoidNeighbors = steerToAvoidNeighbors;
		SteerToAvoidObstacles = steerToAvoidObstacles;
		SteerToStayOnPath = steerToStayOnPath;
		SteerForTargetSpeed = steerForTargetSpeed;
		
        vehicle.Obstacles = new ArrayList();
			// TODO: Should happen in vehicle constructor and should be a property
	}
	
	
	
	public override Vehicle Vehicle
	{
		get
		{
			return vehicle;
		}
	}
	
	
	
	public float MaxSpeed
	{
		get
		{
			return maxSpeed;
		}
		set
		{
			maxSpeed = value;
			vehicle.MaxSpeed = maxSpeed;
		}
	}
	
	
	
	public float MaxForce
	{
		get
		{
			return maxForce;
		}
		set
		{
			maxForce = value;
			vehicle.MaxForce = maxForce;
		}
	}
	
	
	
	public float SteerToAvoidNeighbors
	{
		get
		{
			return steerToAvoidNeighbors;
		}
		set
		{
			steerToAvoidNeighbors = value;
			vehicle.SteerToAvoidNeighborsWeight = steerToAvoidNeighbors;
		}
	}
	
	
	
	public float SteerToAvoidObstacles
	{
		get
		{
			return steerToAvoidObstacles;
		}
		set
		{
			steerToAvoidObstacles = value;
			vehicle.SteerToAvoidObstaclesWeight = steerToAvoidObstacles;
		}
	}
	
	
	
	public float SteerToStayOnPath
	{
		get
		{
			return steerToStayOnPath;
		}
		set
		{
			steerToStayOnPath = value;
			vehicle.SteerToStayOnPathWeight = steerToStayOnPath;
		}
	}
	
	
	
	public float SteerForPursuit
	{
		get
		{
			return steerForPursuit;
		}
		set
		{
			steerToStayOnPath = value;
			vehicle.SteerForPursuitWeight = steerForPursuit;
		}
	}
	
	
	
	public float SteerForTargetSpeed
	{
		get
		{
			return steerForTargetSpeed;
		}
		set
		{
			steerForTargetSpeed = value;
			vehicle.SteerForTargetSpeedWeight = steerForTargetSpeed;
		}
	}



	public Vehicle Target
	{
		get
		{
			return target;
		}
	}



	public void SetTarget( Vehicle target, float steeringWeight )
	{
		this.target = target;
		SteerForPursuit = steeringWeight;
	}
	
	
	
	public void ClearTarget()
	{
		SetTarget( null, 0.0f );
	}



	public void Update()
	{
	    vehicle.Update(Time.deltaTime);
	}
	
	
	
	public Obstacle GetObstacle( GameObject gameObject )
	{
		Obstacle obstacle;
		int id = gameObject.GetInstanceID();
		Component[] colliders;
		float radius = 0.0f, currentRadius;
		
		if( !obstacles.ContainsKey( id ) )
		{
			colliders = gameObject.GetComponentsInChildren( typeof( Collider ) );
			
			if( colliders == null )
			{
				Debug.LogError( "Obstacle '" + gameObject.name + "' has no colliders" );
				return null;
			}
			
			foreach( Collider collider in colliders )
			{
				if( collider.isTrigger )
				{
					continue;
				}
				
				currentRadius = Mathf.Abs( ( gameObject.transform.position - ( collider.transform.position + collider.bounds.center ) ).x ) + collider.bounds.extents.x;
				currentRadius *= gameObject.transform.localScale.x;
				//currentRadius = gameObject.transform.localScale.x / 2.0f;
				
				if( currentRadius > radius )
				{
					radius = currentRadius;
				}
			}
			obstacle = new SphericalObstacle( radius, gameObject.transform.position );
		}
		else
		{
			obstacle = obstacles[ id ] as Obstacle;
		}
		
		return obstacle;
	}
	
	
	
	public void OnRadarEnter( Collider other, Radar sender )
	{
		VehicleBehaviour vehicleBehaviour;
		Obstacle obstacle;

		if( ( 1 << other.gameObject.layer & obstacleLayers ) > 0 )
		{
			obstacle = GetObstacle( other.gameObject );
			if( obstacle != null )
			{
				vehicle.Obstacles.Add( obstacle );
			}
		}
		else
		{		
			vehicleBehaviour = other.GetComponent( typeof( VehicleBehaviour ) ) as VehicleBehaviour;
			if( vehicleBehaviour != null )
			{
        		vehicle.Neighbors.Add( vehicleBehaviour.Vehicle );
			}
		}
	}
	
	
	
	public void OnRadarExit( Collider other, Radar sender )
	{
		VehicleBehaviour vehicleBehaviour;
		Obstacle obstacle;
		
		if( ( 1 << other.gameObject.layer & obstacleLayers ) > 0 )
		{
			obstacle = GetObstacle( other.gameObject );
			if( obstacle != null )
			{
				vehicle.Obstacles.Remove( obstacle );
			}
		}
		else
		{
			vehicleBehaviour = other.GetComponent( typeof( VehicleBehaviour ) ) as VehicleBehaviour;
			if( vehicleBehaviour != null )
			{
        		vehicle.Neighbors.Remove( vehicleBehaviour.Vehicle );
			}
		}
	}
	
	
	
	public void OnRadarStay( Collider other, Radar sender )
	{
		
	}
	
	
	public void OnGUI()
	{
		GUILayout.Label( "vehicle.Obstacles.Count: " + vehicle.Obstacles.Count );
		GUILayout.Label( "vehicle.Neighbors.Count: " + vehicle.Neighbors.Count );
		GUILayout.Label( "" );
		GUILayout.Label( "vehicle.avoidNeighbors: " + vehicle.AvoidNeighbors );
		GUILayout.Label( "vehicle.avoidObstacles: " + vehicle.AvoidObstacles );
		GUILayout.Label( "vehicle.stayOnPath: " + vehicle.StayOnPath );
		GUILayout.Label( "vehicle.pursuit: " + vehicle.Pursuit );
		GUILayout.Label( "vehicle.targetSpeed: " + vehicle.TargetSpeed );
	}

}