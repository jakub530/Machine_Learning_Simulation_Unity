using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy_Spawning : MonoBehaviour
{
    public GameObject Player_Template;
    public GameObject Obstacle_Template;
    public bool Reset_Level;

    public AI_Flock AI_Flock_All;
    public Obstacles Enemies;


    

    public class AI_Flock
    {
        
        List<AI_Player> AI_Player_List;
        int Player_Number = 50;
        int Input_Layer_Size;
        int Output_Layer_Size;
        List<int> Hidden_Layer_Sizes;
        GameObject Player_Template;
        public bool Reset_Obstacles_Flag;
        public int  Gen_Counter;

        public bool Reset_Flag
        {
            get { return this.Reset_Obstacles_Flag; }
            set { Reset_Obstacles_Flag = value; }
        }



        public AI_Flock(int _Input_Layer_Size, int _Output_Layer_Size, List<int> _Hidden_Layer_Sizes, GameObject _Player_Template)
        {
            this.Gen_Counter = 0;
            this.AI_Player_List = new List<AI_Player>();
            AI_Player Player_Add;

            for (int Player_Index = 0; Player_Index < Player_Number; Player_Index++)
            {
                Player_Add = new AI_Player(_Input_Layer_Size, _Output_Layer_Size, _Hidden_Layer_Sizes, _Player_Template);
                Player_Add.ai_player_body.name = "Gen 0: " + (Player_Index + 1);
                this.AI_Player_List.Add(Player_Add);
            }
            this.Input_Layer_Size = _Input_Layer_Size;
            this.Output_Layer_Size = _Output_Layer_Size;
            this.Hidden_Layer_Sizes = _Hidden_Layer_Sizes;
            this.Player_Template = _Player_Template;
        }

        public int Get_Flock_Size()
        {
            return this.AI_Player_List.Count;
        }

        public void Trim_Flock()
        {
            List<int> AI_Player_To_Destroy = new List<int>();
            for (int Player_Index = 0; Player_Index < this.AI_Player_List.Count; Player_Index++)
            {
                //Debug.Log("Checking Players for Destruction");
                if (AI_Player_List[Player_Index].AI_Player_Destruction())
                {
                    //Debug.Log("Players should be getting destroyed");
                    AI_Player_To_Destroy.Add(Player_Index);
                }
            }

            for (int i = AI_Player_To_Destroy.Count - 1; i >= 0; i--)
            {
                GameObject AI_Player = this.AI_Player_List[AI_Player_To_Destroy[i]].Get_AI_Player_Object();
                this.AI_Player_List.RemoveAt(AI_Player_To_Destroy[i]);
                Destroy(AI_Player);

            }
        }

        public void Move_Flock()
        {
            for (int Player_Index = 0; Player_Index < this.AI_Player_List.Count; Player_Index++)
            {
                this.AI_Player_List[Player_Index].Update_Player();
            }
        }


        public void Update_Flock()
        {
            Trim_Flock();
            Regenerate_Flock();

            Move_Flock();
        }

        public void Regenerate_Flock()
        {

            int Player_Multiplier = 10;
            
            if(AI_Player_List.Count < 10)
            {
                this.Gen_Counter++;
                Reset_Obstacles_Flag = true;
                for (int Player_Index=0; Player_Index < AI_Player_List.Count; Player_Index++)
                {
                    this.AI_Player_List[Player_Index].Get_AI_Player_Object().GetComponent<Rigidbody2D>().position = new Vector2(0, 0);
                }

                AI_Player Player_Add;
                int Temp_Count = AI_Player_List.Count;
                int Total_Index = 1;
                for (int Parrent_Index = 0; Parrent_Index < Temp_Count; Parrent_Index++)
                {
                    for (int Player_Index = 0; Player_Index < Player_Multiplier; Player_Index++)
                    {
                        Player_Add = new AI_Player(this.Input_Layer_Size, this.Output_Layer_Size, this.Hidden_Layer_Sizes, this.Player_Template);
                        Player_Add.ai_player_body.name = "Gen "+ Gen_Counter + ": " + this.AI_Player_List[Parrent_Index].ai_player_body.name;
                       // Debug.Log("Test M");
                        Player_Add.Neural_Net = this.AI_Player_List[Parrent_Index].Neural_Net.Permutate_Net();
                        this.AI_Player_List.Add(Player_Add);
                        Total_Index = Total_Index + 1;

                    }
                }
            }

            
        }



        public class AI_Player
        {
            Neural_Net Controlling_Net;
            GameObject AI_Player_Body;
            Rigidbody2D AI_Player_Rigidbody;
            bool Destruction_Enable;
            bool Enable_Update;

            public AI_Player(int _Input_Layer_Size, int _Output_Layer_Size, List<int> _Hidden_Layer_Sizes, GameObject Player_Template)
            {
                this.Controlling_Net = new Neural_Net(_Input_Layer_Size, _Output_Layer_Size, _Hidden_Layer_Sizes);
                this.AI_Player_Body = Instantiate(Player_Template) as GameObject;
                this.AI_Player_Rigidbody = this.AI_Player_Body.GetComponent<Rigidbody2D>();
                this.AI_Player_Rigidbody.position = new Vector2(0, 0);
                this.Destruction_Enable = true;
                this.Enable_Update = true;
            }

            public GameObject ai_player_body
            {
                get { return this.AI_Player_Body; }
                set { AI_Player_Body = value; }
            }

            public Neural_Net Neural_Net
            {
                get { return this.Controlling_Net; }
                set { Controlling_Net = value; }
            }


            public Neural_Net Get_Network()
            {
                return this.Controlling_Net;
            }

            public void Set_Network(Neural_Net Input_Net)
            {
                this.Controlling_Net = Input_Net;
            }

            public void Permutate_Player(AI_Player Parent_Player)
            {
                this.Controlling_Net = Parent_Player.Controlling_Net.Permutate_Net();
            }

            public GameObject Get_AI_Player_Object()
            {
                return this.AI_Player_Body;
            }

            private List<float> Ray_Cast()
            {
                int RayNumber = 8;
                float Max_Distance = 3.0f;
                List<float> Distances = new List<float>();
                RaycastHit2D hit;
                float Angle;

                for (float i = 0; i < RayNumber; i++)
                {
                    Angle = i * 2 * Mathf.PI / RayNumber;
                    hit = Physics2D.Raycast(AI_Player_Rigidbody.position, new Vector2(Mathf.Sin(Angle), Mathf.Cos(Angle)), Max_Distance);
                    Debug.DrawRay(AI_Player_Rigidbody.position, new Vector2(Max_Distance * Mathf.Sin(Angle), Max_Distance * Mathf.Cos(Angle)), Color.red);

                    Distances.Add(Transform_Length(hit.distance, Max_Distance));
                }
                return Distances;
            }

            void AI_Player_Movement()
            {
                float Max_Rotate_Speed = 500.0f;
                float Max_Linear_Speed = 300.0f;
                List<float> Output_Values = new List<float>();

                Output_Values = this.Controlling_Net.Calculate_Net_Output(Ray_Cast());
                float Linear_Velocity = Output_Values[0];

                //this.AI_Player_Rigidbody.angularVelocity = Max_Rotate_Speed * Output_Values[1];
                //this.AI_Player_Rigidbody.velocity = new Vector2(Max_Linear_Speed * Linear_Velocity * Mathf.Cos(this.AI_Player_Rigidbody.rotation * Mathf.PI * 2 / 360), Max_Linear_Speed * Linear_Velocity * Mathf.Sin(this.AI_Player_Rigidbody.rotation * Mathf.PI * 2 / 360));
                this.AI_Player_Rigidbody.velocity = new Vector2(Max_Linear_Speed * Output_Values[0], Max_Linear_Speed * Output_Values[1]);
            }

            private float Transform_Length(float Input, float Max_Distance)
            {
                float Output;
                if (Input == 0)
                {
                    Output = 0;
                }
                else
                {
                    Output = Max_Distance - Input;
                }
                return Output;
            }

            public void Update_Player()
            {
                if (this.Enable_Update)
                {
                    AI_Player_Movement();

                }
            }
            public bool AI_Player_Destruction()
            {
                if (Destruction_Enable)
                {
                    int numColliders = 1;
                    Collider2D[] colliders = new Collider2D[numColliders];
                    ContactFilter2D contactFilter = new ContactFilter2D();
                    contactFilter.SetLayerMask(LayerMask.GetMask("Enemy Layer"));

                    int colliderCount = AI_Player_Rigidbody.OverlapCollider(contactFilter, colliders);

                    if (colliderCount != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
        }
    }

    public class Obstacles
    {
        List<GameObject> Obstacle_List;
        GameObject Obstacle_Template;
        bool restart_level;
        

        public Obstacles(GameObject _Obstacle_Template)
        {
            this.Obstacle_Template = _Obstacle_Template;
            this.Obstacle_List = new List<GameObject>();
            this.restart_level = false;
        }

        public bool Restart_Level
        {
            get { return this.restart_level; }
            set { restart_level = value; }
        }

        public void Generate_Obstacles()
        {
            int Spawn_Frequency = 5;
            if (Time.frameCount % Spawn_Frequency == 0)
            {
                GameObject Obstacle_Clone;
                
                Obstacle_Clone = Instantiate(Obstacle_Template) as GameObject;
                Obstacle_Clone.GetComponent<Rigidbody2D>().position = Position_Randomization();
                Obstacle_Clone.GetComponent<Rigidbody2D>().velocity = Velocity_Randomization();
                this.Obstacle_List.Add(Obstacle_Clone);
            }
        }

        public void Update_Obstacles()
        {
            if(this.restart_level == true)
            {
                Destroy_All_Obstacle();
                this.restart_level = false;
            }
            Generate_Obstacles();
            
            Destroy_Obstacles();
        }

        public void Destroy_All_Obstacle()
        {
            for(int Obstacle_Index = 0; Obstacle_Index < this.Obstacle_List.Count; Obstacle_Index++)
            {
                Destroy(this.Obstacle_List[Obstacle_Index]);
            }
            Obstacle_List.Clear();
        }



        private Vector2 Position_Randomization()
        {
            float Radius = 25;

            float Angle = Random.Range(0, 2 * Mathf.PI);
            float position_x = Radius * Mathf.Sin(Angle);
            float position_y = Radius * Mathf.Cos(Angle);

            return new Vector2(position_x, position_y);
        }

        private Vector2 Velocity_Randomization()
        {
            float Velocity = 5.0f;

            float Angle = Random.Range(0, 2 * Mathf.PI);
            float velocity_x = Velocity * Mathf.Sin(Angle);
            float velocity_y = Velocity * Mathf.Cos(Angle);

            return new Vector2(velocity_x, velocity_y);
        }

        public void Destroy_Obstacles()
        {
            float Max_Radius = 40;
            List<int> Obstacles_To_Destroy = new List<int>();
            int Obstacle_Index = 0;

            foreach (GameObject Obstacle in this.Obstacle_List)
            {
                if (Obstacle.GetComponent<Rigidbody2D>().position.magnitude > Max_Radius)
                {
                    Obstacles_To_Destroy.Add(Obstacle_Index);
                }
                Obstacle_Index++;
            }

            for (int i = Obstacles_To_Destroy.Count - 1; i >= 0; i--)
            {
                GameObject Obstacle = this.Obstacle_List[Obstacles_To_Destroy[i]];
                this.Obstacle_List.RemoveAt(Obstacles_To_Destroy[i]);
                Destroy(Obstacle);

            }
        }

    }

    public class Neural_Net
    {
        //Neural_Net_Layer Input_Layer;
        Neural_Net_Layer Output_Layer;

        List<int> Hidden_Layer_Sizes;
        List<Neural_Net_Layer> Hidden_Layers;
        int Input_Layer_Size;


        public Neural_Net(int _Input_Layer_Size, int _Output_Layer_Size, List<int> _Hidden_Layer_Sizes)
        {
            //this.Input_Layer = new Neural_Net_Layer(_Input_Layer_Sizes);
            this.Input_Layer_Size = _Input_Layer_Size;
            this.Hidden_Layer_Sizes = _Hidden_Layer_Sizes;
            this.Hidden_Layers = new List<Neural_Net_Layer>();
            Neural_Net_Layer Hidden_Layer;
            string Layer_ID;
            for (int Hidden_Layer_Index = 0; Hidden_Layer_Index < _Hidden_Layer_Sizes.Count; Hidden_Layer_Index++)
            {
                if (Hidden_Layer_Index == 0)
                {
                    Layer_ID = "Hidden Layer 1 ";
                    Hidden_Layer = new Neural_Net_Layer(_Hidden_Layer_Sizes[Hidden_Layer_Index], _Input_Layer_Size, Layer_ID);
                }
                else
                {
                    Hidden_Layer = new Neural_Net_Layer(_Hidden_Layer_Sizes[Hidden_Layer_Index], _Hidden_Layer_Sizes[Hidden_Layer_Index - 1], "Hidden Layer " + (Hidden_Layer_Index + 1));
                }
                this.Hidden_Layers.Add(Hidden_Layer);
            }
            this.Output_Layer = new Neural_Net_Layer(_Output_Layer_Size, _Hidden_Layer_Sizes[_Hidden_Layer_Sizes.Count - 1], "Output Layer");
        }

        public List<float> Calculate_Net_Output(List<float> Input_Values)
        {
            if (Input_Values.Count == this.Input_Layer_Size)
            {
                List<float> Calculated_Values = new List<float>();
                List<float> Temporary_Values = Input_Values;

                for (int Hidden_Layer_Index = 0; Hidden_Layer_Index < this.Hidden_Layers.Count; Hidden_Layer_Index++)
                {
                    Calculated_Values = this.Hidden_Layers[Hidden_Layer_Index].Generate_Layer_Output(Temporary_Values);
                    Temporary_Values = Calculated_Values;
                }

                Calculated_Values = this.Output_Layer.Generate_Layer_Output(Temporary_Values);
                return Calculated_Values;
            }
            else
            {
                List<float> Empty_List = new List<float>();
                for (int Output_Layer_Node_Index = 0; Output_Layer_Node_Index < Output_Layer.Get_Layer_Size(); Output_Layer_Node_Index++)
                {
                    Empty_List.Add(0.0f);
                }
                Debug.LogError("Incorrect Size of the input");
                Debug.LogError("Should be " + this.Input_Layer_Size + " but is " + Input_Values.Count);
                return Empty_List;
            }
        }

        public void Debug_Layer_Weights()
        {
            Debug.Log("Weights of neural net are as follows:");
            for (int Hidden_Layer_Index = 0; Hidden_Layer_Index < this.Hidden_Layers.Count; Hidden_Layer_Index++)
            {
                this.Hidden_Layers[Hidden_Layer_Index].Debug_Layer_Weights(" Hidden Layer " + Hidden_Layer_Index + " ");
            }
            this.Output_Layer.Debug_Layer_Weights(" Output_Layer ");
        }

        public Neural_Net Permutate_Net ()
        {
            Neural_Net Return_Net = new Neural_Net(this.Input_Layer_Size, this.Output_Layer.Get_Layer_Size(), this.Hidden_Layer_Sizes);
           // Debug.Log("Test 0");
            for (int Hidden_Layer_Index = 0; Hidden_Layer_Index < this.Hidden_Layers.Count; Hidden_Layer_Index++)
            {
               // Debug.Log("Test 1");
                Return_Net.Hidden_Layers[Hidden_Layer_Index] = this.Hidden_Layers[Hidden_Layer_Index].Permutate_Layer();
            }
          //  Debug.Log("Test 2");
            Return_Net.Output_Layer = this.Output_Layer.Permutate_Layer();
          //  Debug.Log("Test 3");
            return Return_Net;
        }

        public void Set_Debug_Mode(bool Input_Debug)
        {
            for (int Hidden_Layer_Index = 0; Hidden_Layer_Index < this.Hidden_Layers.Count; Hidden_Layer_Index++)
            {
                this.Hidden_Layers[Hidden_Layer_Index].Set_Debug_Mode(Input_Debug);
            }
            this.Output_Layer.Set_Debug_Mode(Input_Debug);
        }



        
        

        public class Neural_Net_Layer
        {
            int Layer_Size;
            int Prev_Layer_Size;
            List<Neural_Net_Node> Layer_Nodes;
            string Layer_ID;

            public Neural_Net_Layer(int _Layer_Size, int _Prev_Layer_Size, string _Layer_ID)
            {
                this.Layer_Size = _Layer_Size;
                this.Layer_ID = _Layer_ID;
                Generate_Layer_Nodes(_Prev_Layer_Size);
            }

            public List<Neural_Net_Node> layer_nodes
            {
                get { return this.Layer_Nodes; }
                set { Layer_Nodes = value; }
            }

            public Neural_Net_Layer Permutate_Layer()
            {
                Neural_Net_Layer Return_Layer = new Neural_Net_Layer(this.Layer_Size,this.Prev_Layer_Size,this.Layer_ID);
                for (int Node_Index = 0; Node_Index < this.Layer_Size; Node_Index++)
                {
                    Return_Layer.Layer_Nodes[Node_Index] = this.Layer_Nodes[Node_Index].Permutate_Node();
                }
                return Return_Layer;
            }

            public int Get_Layer_Size()
            {
                return this.Layer_Size;
            }

            public void Generate_Layer_Nodes(int _Prev_Layer_Size)
            {
                string Node_ID;
                this.Layer_Nodes = new List<Neural_Net_Node>();
                for (int i = 0; i < this.Layer_Size; i++)
                {
                    Node_ID = this.Layer_ID + " Node Number " + i;
                    this.Layer_Nodes.Add(new Neural_Net_Node(_Prev_Layer_Size, Node_ID));
                }
            }

            public void Change_This_Layer()
            {
                for (int Node_Index = 0; Node_Index < this.Layer_Size; Node_Index++)
                {
                    this.Layer_Nodes[Node_Index].Change_This_Node();
                }
            }

            public List<float> Generate_Layer_Output(List<float> Input_Values)
            {
                List<float> Layer_Output = new List<float>();
                for (int Node_Index = 0; Node_Index < this.Layer_Size; Node_Index++)
                {
                    Layer_Output.Add(this.Layer_Nodes[Node_Index].Generate_Node_Output(Input_Values));
                }
                return Layer_Output;
            }

            public void Debug_Layer_Weights(string Message)
            {
                Debug.Log("Layer " + Message + "has Nodes with weights as follows:");
                for (int Node_Index = 0; Node_Index < this.Layer_Size; Node_Index++)
                {
                    this.Layer_Nodes[Node_Index].Debug_Node_Weights("at index " + Node_Index + " ");
                }
            }




            public void Set_Debug_Mode(bool Input_Debug)
            {
                for (int Node_Index = 0; Node_Index < this.Layer_Size; Node_Index++)
                {
                    this.Layer_Nodes[Node_Index].Debug_Mode = Input_Debug;
                }
            }
            

            public class Neural_Net_Node
            {
                private int Input_Size;
                private List<float> node_weights;
                private bool debug;
                private string node_id;

                public List<float> Node_Weight
                {
                    get { return this.node_weights; }
                }

                public bool Debug_Mode
                {
                    get { return this.debug; }
                    set { debug = value; }
                }

                public string Node_ID
                {
                    get { return this.node_id; }
                    set { node_id = value; }
                }

                public Neural_Net_Node(int _Input_Size, string _Node_ID)
                {
                    this.Input_Size = _Input_Size;
                    this.debug = false;
                    Generate_Node_Weights();
                    this.node_id = _Node_ID;
                }

                public void Generate_Node_Weights()
                {
                    this.node_weights = new List<float>();
                    for (int i = 0; i < this.Input_Size + 1; i++)
                    {
                        this.node_weights.Add(Random.Range(-1.0f, 1.0f));
                    }
                }

                public float Generate_Node_Output(List<float> Input)
                {
                    float Output = 0.0f;
                    Input.Add(0.01f);
                    for (int Input_Index = 0; Input_Index < node_weights.Count; Input_Index++)
                    {
                        Output += Input[Input_Index] * node_weights[Input_Index];
                    }
                    if (this.debug)
                    {
                        Debug.Log("Output of the node: " + this.node_id + " is " + Output);
                    }
                    return Decision_Function(Output);
                }



                private float Decision_Function(float Input)
                {
                    float Output = Sigmoid(Input) - 0.5f;
                    return Output;
                }

                public void Change_This_Node()
                {
                    for (int Node_Index = 0; Node_Index < this.node_weights.Count; Node_Index++)
                    {
                        this.node_weights[Node_Index] += Random.Range(-0.1f, 0.1f);
                    }
                }

                public Neural_Net_Node Permutate_Node()
                {
                    Neural_Net_Node Return_Node = new Neural_Net_Node(this.Input_Size, this.Node_ID);

                    for (int Node_Index = 0; Node_Index < this.node_weights.Count; Node_Index++)
                    {
                        Return_Node.node_weights[Node_Index] = this.node_weights[Node_Index] + Random.Range(-0.1f, 0.1f);
                    }
                    return Return_Node;
                }

                public void Debug_Node_Weights(string Message)
                {
                    for (int Node_Index = 0; Node_Index < this.node_weights.Count; Node_Index++)
                    {
                        Debug.Log("Node " + Message + " has weight of " + node_weights[Node_Index] + " as input " + Node_Index);
                    }
                }

                private float Sigmoid(float Input)
                {
                    return 1.0f / (1.0f + Mathf.Exp(Input));
                }
            }
        }
    }




    void Debug_List(string Message, List<float> List)
    {
        for (int index = 0; index < List.Count; index++)
        {
            Debug.Log(Message + " index " + index + " " + List[index]);
        }
    }

    public int Fitness;
    public int Temporary_Passed_Frames;
    public int Gen_Count;

    void Start()
    {
        Application.targetFrameRate =  500;
        List<int> Layer_Sizes = new List<int>();
        Layer_Sizes.Add(6);
        Layer_Sizes.Add(4);
        AI_Flock_All = new AI_Flock(8, 2, Layer_Sizes, Player_Template);
        Enemies = new Obstacles(Obstacle_Template);
        Temporary_Passed_Frames = 0;
        Gen_Count = 0;
    }

    void Update()
    {
        Enemies.Update_Obstacles();
        AI_Flock_All.Update_Flock();
        if (AI_Flock_All.Reset_Flag == true)
        {
            Fitness = Time.frameCount - Temporary_Passed_Frames;
            Temporary_Passed_Frames = Time.frameCount;
            Debug.Log("Fitness of Gen: "+ Gen_Count +" is " + Fitness);
            Enemies.Restart_Level = AI_Flock_All.Reset_Flag;
            AI_Flock_All.Reset_Flag = false;
            Gen_Count++;
        }
    }

}




