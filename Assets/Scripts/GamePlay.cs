using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay : MonoBehaviour
{
    // You can only use Manager.Instance.SetTextField(int textFieldIndex, string text) function
    // to change the text fields in the game 
    // e.g. for (int i = 0; i < 16; i++) { Manager.Instance.SetTextField(i, i.ToString()); }
    bool player_has_played = false;
    bool game_over = false;
    int[] player_state = new int[14];
    int[] cpu_state = new int[14];

    int computerTurn;

    // Main class which takes state of the game as configuration and manipulate according to rules
    public class Node
    {
        // for player, 1 is computer and 0 is human
        // Constructor for child nodes
        public Node(int defIndex, int depth, bool parentStat, int[] configuration)
        {
            // TODO: Let the node decide itself if it's a max/min node
            // TODO: Check if max/min have any significance inside the node first
            // isLast_ = !(player);
            index_ = defIndex;
            depth_ = depth;
            player_ = parentStat;
            lastConfiguration_ = copyConfiguration(configuration);
            returnConfig_ = calPresentConfiguration();
            nodeValue_ = getValue();
        }

        // Constructor for root of the tree
        public Node()
        {
            index_ = -1;
            depth_ = -1;
            player_ = true;
            returnConfig_ = getInitialConfig();
            nodeValue_ = getValue();
        }
        ~Node() { }
        public int getDepth() { return depth_; }
        public int getIndex() { return index_; }
        public int[] getPresentConfiguration() { return returnConfig_; }
        public bool getNodeStat() { return nodeStat_; }
        public int[] getLastConfiguration() { return lastConfiguration_; }
        private bool player_;
        private int depth_;
        private int index_;
        private float nodeValue_;
        private int[] presentConfiguration_;
        private int[] lastConfiguration_;
        private int[] returnConfig_;
        private bool nodeStat_;

        private int[] copyConfiguration(int[] conf)
        {
            int[] retConf = new int[14];
            for (int i = 0; i < 14; i++)
                retConf[i] = conf[i];
            return retConf;
        }

        // Heuristic Value computation
        public float getValue()
        {
            int sum = 0;
            for (int i = 7; i < 13; i++)
            {
                sum += returnConfig_[i];
            }
            float h1 = (returnConfig_[13] - returnConfig_[6]) / 48f;
            int sum2 = 0;
            for (int i = 0; i < 6; i++)
            {
                sum2 += returnConfig_[i];
            }
            float h2 = ((float)sum) / 48f;
            float h3 = ((float)sum2) / 48f;
            float h4 = (returnConfig_[12] + returnConfig_[11]) / 48f;
            float h5 = (returnConfig_[7] + returnConfig_[8]) / 48f;
            float h6 = (returnConfig_[9] + returnConfig_[10]) / 48f;

            return ((h1 * 0.84375f) + (h2 * 0.5625f) - (h3 * 0.5625f) + (h4 * 0.5f) + (h5 * 0.46875f) + (h6 * 0.5f));
        }

        private void setPresentConfiguration(int[] lastConfiguration, int stepTaken, bool player)
        {
            if (player_)
                presentConfiguration_ = updateConfiguration(lastConfiguration, stepTaken + 7);
            else
                presentConfiguration_ = updateConfiguration(lastConfiguration, stepTaken);
        }

        private int[] updateConfiguration(int[] lastConfiguration, int stepTaken)
        {
            bool isComputer = (stepTaken > 6);
            //bool addPebbles = false;
            int numElements = lastConfiguration[stepTaken];
            int checkRepeatChance = numElements + stepTaken;
            lastConfiguration[stepTaken] = 0;

            if (isComputer)
            {
                for (int i = 0; i < numElements; i++)
                {
                    if ((stepTaken + i + 1) != 6)
                    {
                        if (stepTaken + i + 1 > 13)
                            stepTaken = -(i + 1);
                        lastConfiguration[stepTaken + i + 1] += 1;
                    }
                    if (i == numElements - 1 && stepTaken + i + 1 == 13)
                    {
                        nodeStat_ = true;
                    }
                    else if (i == numElements - 1 && stepTaken + i + 1 > 6 && stepTaken + i + 1 < 13)
                    {
                        // If the last pebble falls in empty pit and the opposite pit is not empty
                        if (lastConfiguration[stepTaken + i + 1] == 1 && lastConfiguration[12 - (stepTaken + i + 1)] != 0)
                        {
                            lastConfiguration[13] += lastConfiguration[12 - (stepTaken + i + 1)] + lastConfiguration[stepTaken + i + 1];
                            lastConfiguration[stepTaken + i + 1] = 0;
                            lastConfiguration[12 - (stepTaken + i + 1)] = 0;
                        }
                        nodeStat_ = false;

                    }


                }
                return lastConfiguration;
            }
            for (int i = 0; i < numElements; i++)
            {
                if ((stepTaken + i + 1) != 13)
                {
                    if (stepTaken + i + 1 > 13)
                        stepTaken = -(i + 1);
                    lastConfiguration[stepTaken + i + 1] += 1;
                }
                // Check if the last pebble falls in big pit
                if (i == numElements - 1 && stepTaken + i + 1 == 6)
                {
                    // player gets other turn
                    nodeStat_ = false;
                }

                // Condition for last element falling in player's side
                else if (i == numElements - 1 && stepTaken + i + 1 >= 0 && stepTaken + i + 1 < 6)
                {
                    // If the last pebble falls in empty pit and the opposite pit is not empty
                    if (lastConfiguration[stepTaken + i + 1] == 1 && lastConfiguration[12 - (stepTaken + i + 1)] != 0)
                    {
                        lastConfiguration[6] += lastConfiguration[12 - (stepTaken + i + 1)] + lastConfiguration[stepTaken + i + 1];
                        lastConfiguration[stepTaken + i + 1] = 0;
                        lastConfiguration[12 - (stepTaken + i + 1)] = 0;
                        nodeStat_ = true;
                    }
                }

            }
            return lastConfiguration;
        }
        private int[] calPresentConfiguration()
        {
            setPresentConfiguration(lastConfiguration_, index_, player_);
            return presentConfiguration_;
        }
        private int[] getInitialConfig()
        {
            int[] retConfig = new int[14];
            for (int i = 0; i < 14; i++)
            {
                retConfig[i] = int.Parse(Manager.Instance.TextFields[i].text);
            }
            return retConfig;
        }
    }
    // Helper function to compute max Value and keep track of index
    private float[] maxValue(float value, float bestValue, float[] choice, int stepTaken)
    {
        float[] ret = new float[2];
        if (value >= bestValue)
        {
            ret[0] = value;
            ret[1] = (float)stepTaken;
            return ret;
        }
        return choice;
    }
    // Helper function to compute min Value and keep track of index
    private float[] minValue(float value, float bestValue, float[] choice, int stepTaken)
    {
        float[] ret = new float[2];
        if (value < bestValue)
        {
            ret[0] = value;
            ret[1] = (float)stepTaken;
            return ret;
        }
        return choice;
    }
    // computes if node is terminal
    private bool terminalNode(bool maxPlayer, Node node, int depth)
    {
        if (depth > 8)
            return true;
        int j = 0;
        int iter = 7;
        if (maxPlayer == true)
            iter = 0;

        for (int i = 0 + iter; i < 6 + iter; i++)
        {
            if ((node.getPresentConfiguration())[i] == 0)
                j++;
        }
        if (j == 6)
            return true;
        return false;
    }

    // While initializing minimax pass the root node where properties are computed before in main function
    // Implementation of miniMax
    private float miniMax(Node node, bool maxPlayer, int depth)//for the first call values are (presentNode, true, 0)
    {

        depth++;
        // Check Termination condition
        if (terminalNode(maxPlayer, node, depth)) { return node.getValue(); }

        int j = 0;
        if (maxPlayer)
            j = 1;

        // To return value and index of the maximum value node
        float[] bestChange = new float[2];
        bestChange[0] = 0f;
        bestChange[1] = 0f;

        List<Node> nodeChild = new List<Node>();
        // Add possible nodes to the tree, do not add if the pit is empty
        for (int i = 0; i < 6; i++)
            if (int.Parse(Manager.Instance.TextFields[7 * j + i].text) != 0)
            {
                nodeChild.Add(new Node(i, depth, maxPlayer, node.getPresentConfiguration()));
            }

        if (maxPlayer)
        {
            float bestValue = Mathf.NegativeInfinity;
            for (int i = 0; i < nodeChild.Count; i++)
            {
                float Value = miniMax(nodeChild[i], nodeChild[i].getNodeStat(), depth);
                bestChange = maxValue(Value, bestValue, bestChange, nodeChild[i].getIndex());
                bestValue = bestChange[0];

            }
            if (depth == 1)
                return bestChange[1];
            return bestChange[0];
        }
        else
        {
            float bestValue = Mathf.Infinity;
            for (int i = 0; i < nodeChild.Count; i++)
            {
                float Value = miniMax(nodeChild[i], nodeChild[i].getNodeStat(), depth);
                bestChange = minValue(Value, bestValue, bestChange, nodeChild[i].getIndex());
                bestValue = bestChange[0];
            }
            //return bestValue;
            if (depth == 1)
                return bestChange[1];
            return bestChange[0];
        }
    }


    // Use this for initialization
    void Start()
    {
        // GameBoard initialization
        game_over = false;
        for (int i = 0; i < 15; i++) //@D set all the values to intial values
        {
            if (i == 6 || i == 13 || i == 14)
                Manager.Instance.SetTextField(i, 0.ToString());
            else
                Manager.Instance.SetTextField(i, 4.ToString());
        }
        Manager.Instance.SetTextField(15, "User's Turn".ToString());
    }

    // Update is called once per frame
    void Update()
    {
        Manager.Instance.SetTextField(14, int.Parse(Manager.Instance.TextFields[6].text).ToString());
    }

    public string scoreComparer()
    { //@D compare the scores and tell who is the winner
        if (int.Parse(Manager.Instance.TextFields[6].text) > int.Parse(Manager.Instance.TextFields[13].text))
            return "Player wins";
        if (int.Parse(Manager.Instance.TextFields[6].text) == int.Parse(Manager.Instance.TextFields[13].text))
            return "its a tie";
        return "CPU wins";
    }

    public bool isGameOver()
    {
        int player = 0;
        int cpu = 0;

        for (int i = 0; i < 6; i++)
            if (int.Parse(Manager.Instance.TextFields[i].text) == 0)
                player++;
        for (int i = 7; i < 13; i++)
            if (int.Parse(Manager.Instance.TextFields[i].text) == 0)
                cpu++;

        if (cpu == 6 || player == 6)
            return true;

        return false;
    }

    public bool player_check()
    { //check end condion on player side that is there are no zeros
        bool player_zero_check = false;
        if (int.Parse(Manager.Instance.TextFields[0].text) == 0 && int.Parse(Manager.Instance.TextFields[1].text) == 0 && int.Parse(Manager.Instance.TextFields[2].text) == 0 && int.Parse(Manager.Instance.TextFields[3].text) == 0 && int.Parse(Manager.Instance.TextFields[4].text) == 0 && int.Parse(Manager.Instance.TextFields[5].text) == 0)
        {
            player_zero_check = true;
        }
        return player_zero_check;

    }
    public bool cpu_check() //check end condition on CPU side
    {
        bool cpu_zero_check = false;
        if (int.Parse(Manager.Instance.TextFields[7].text) == 0 && int.Parse(Manager.Instance.TextFields[8].text) == 0 && int.Parse(Manager.Instance.TextFields[9].text) == 0 && int.Parse(Manager.Instance.TextFields[10].text) == 0 && int.Parse(Manager.Instance.TextFields[11].text) == 0 && int.Parse(Manager.Instance.TextFields[12].text) == 0)
        {
            cpu_zero_check = true;
        }
        return cpu_zero_check;

    }

    public void ApplyRestart() // reinitialze the frames
    {
        if ((Manager.Instance.TextFields[15].text) != "Thinking")
        {
            Start();
        }

    }

    public void ApplyUndo()
    {
        //store the previous game state...//for mini max parse the tree accordingly...
        game_over = false;
        if ((Manager.Instance.TextFields[15].text) != "Thinking")
        {
            for (int i = 0; i < 14; i++)
            {
                Manager.Instance.SetTextField(i, player_state[i].ToString());
            }
        }

    }

    public void UserInput(int pitNumber)
    {   //add delay
        if (!isGameOver() && Manager.Instance.TextFields[15].text != "Thinking")
        {
            for (int i = 0; i < 14; i++)
                player_state[i] = int.Parse(Manager.Instance.TextFields[i].text);
            // update for undo state
            StartCoroutine(Delay_adder(pitNumber));
        }
        // call delay function, the game rules are there in that function itself
    }

    IEnumerator Delay_adder(int pitNumber)  // player delay function
    {
        int new_copy = pitNumber;
        float wait_time = 0.5f;
        int n = int.Parse(Manager.Instance.TextFields[pitNumber].text);

        if (n != 0)  //so that n is not selected
        {
            Manager.Instance.SetTextField(pitNumber, 0.ToString());
            yield return new WaitForSeconds(wait_time);
            for (int i = 0; i < n; i++)
            {
                if (pitNumber + i + 1 > 12)
                    pitNumber = -i - 1; // remodify the pitnumber

                Manager.Instance.SetTextField(pitNumber + i + 1, (int.Parse(Manager.Instance.TextFields[pitNumber + i + 1].text) + 1).ToString());
                yield return new WaitForSeconds(wait_time); // delay

            }

            if (new_copy + n < 6)
            {
                if (n > 0 && int.Parse(Manager.Instance.TextFields[new_copy + n].text) == 1 && int.Parse(Manager.Instance.TextFields[12 - (new_copy + n)].text) != 0)
                {

                    int n_shift = int.Parse(Manager.Instance.TextFields[12 - (new_copy + n)].text);

                    Manager.Instance.SetTextField(6, (int.Parse(Manager.Instance.TextFields[6].text) + n_shift + (int.Parse(Manager.Instance.TextFields[new_copy + n].text))).ToString());
                    Manager.Instance.SetTextField(12 - (new_copy + n), 0.ToString());
                    Manager.Instance.SetTextField(new_copy + n, 0.ToString());

                }
            }

            else if (new_copy + n >= 13)
            {
                if (int.Parse(Manager.Instance.TextFields[new_copy + n - 13].text) == 1 && int.Parse(Manager.Instance.TextFields[12 - (new_copy + n - 13)].text) != 0)
                {
                    int n_shift = int.Parse(Manager.Instance.TextFields[12 - (new_copy + n - 13)].text);

                    Manager.Instance.SetTextField(6, (int.Parse(Manager.Instance.TextFields[6].text) + n_shift + (int.Parse(Manager.Instance.TextFields[new_copy + n - 13].text))).ToString());
                    Manager.Instance.SetTextField(12 - (new_copy + n - 13), 0.ToString());
                    Manager.Instance.SetTextField(new_copy + n - 13, 0.ToString());
                }

            }

            // check if game is over, if yes then accumulate
            if (isGameOver()) // player end condition check
            {
                int playerSum = 0;
                int cpuSum = 0;
                for (int i = 0; i < 7; i++)
                {
                    playerSum += int.Parse(Manager.Instance.TextFields[i].text);
                    Manager.Instance.SetTextField(i, 0.ToString());
                }
                for (int i = 7; i < 14; i++)
                {
                    cpuSum += int.Parse(Manager.Instance.TextFields[i].text);
                    Manager.Instance.SetTextField(i, 0.ToString());
                }
                Manager.Instance.SetTextField(13, cpuSum.ToString());
                Manager.Instance.SetTextField(6, playerSum.ToString());
                string winner = scoreComparer();
                Manager.Instance.SetTextField(15, winner);
                game_over = true;
            }

            //last pebble is not in the block 6 then cpu turn.
            if ((pitNumber + n) != 6 && game_over == false)
            {
                Manager.Instance.SetTextField(15, "Thinking");
                random_CPU_input();
            }



        }

    }

    IEnumerator Delay_adder_cpu(int pitNumber)
    {
        int new_copy = pitNumber;
        float wait_time = 0.5f;
        int n = int.Parse(Manager.Instance.TextFields[pitNumber].text);
        if (game_over == false)
        {
            for (int i = 0; i < 14; i++)
            {
                cpu_state[i] = int.Parse(Manager.Instance.TextFields[i].text);
            }
        }
        int n_copy = n;
        yield return new WaitForSeconds(1.5f);
        Manager.Instance.SetTextField(pitNumber, 0.ToString());
        yield return new WaitForSeconds(wait_time);
        for (int i = 0; i < n_copy; i++)
        {
            if ((pitNumber + i + 1) != 6) // skip 6 condition
            {
                Manager.Instance.SetTextField(pitNumber + i + 1, (int.Parse(Manager.Instance.TextFields[pitNumber + i + 1].text) + 1).ToString());
                yield return new WaitForSeconds(wait_time);
                if (pitNumber + i + 1 >= 13)
                {
                    pitNumber = -i - 1 - 1;
                }
            }
            else
            {
                n_copy++;
            }
        }

        if (new_copy + n < 13 && int.Parse(Manager.Instance.TextFields[new_copy + n].text) == 1 && int.Parse(Manager.Instance.TextFields[12 - (new_copy + n)].text) != 0)
        {
            int n_shift = int.Parse(Manager.Instance.TextFields[12 - (new_copy + n)].text);
            Manager.Instance.SetTextField(13, (int.Parse(Manager.Instance.TextFields[13].text) + n_shift + (int.Parse(Manager.Instance.TextFields[new_copy + n].text))).ToString());
            Manager.Instance.SetTextField(12 - (new_copy + n), 0.ToString());
            Manager.Instance.SetTextField(new_copy + n, 0.ToString());
        }
        else if (new_copy + n > 19 && int.Parse(Manager.Instance.TextFields[new_copy + n - 13].text) == 1 && int.Parse(Manager.Instance.TextFields[12 - (new_copy + n - 13)].text) != 0)
        {
            int n_shift = int.Parse(Manager.Instance.TextFields[12 - (new_copy + n - 13)].text);
            Manager.Instance.SetTextField(13, (int.Parse(Manager.Instance.TextFields[13].text) + n_shift + (int.Parse(Manager.Instance.TextFields[new_copy + n - 13].text))).ToString());
            Manager.Instance.SetTextField(12 - (new_copy + n - 13), 0.ToString());
            Manager.Instance.SetTextField(new_copy + n - 13, 0.ToString());
        }

        if (isGameOver()) //player end condition check
        {
            int playerSum = 0;
            int cpuSum = 0;
            for (int i = 0; i < 7; i++)
            {
                playerSum += int.Parse(Manager.Instance.TextFields[i].text);
                Manager.Instance.SetTextField(i, 0.ToString());
            }
            for (int i = 7; i < 14; i++)
            {
                cpuSum += int.Parse(Manager.Instance.TextFields[i].text);
                Manager.Instance.SetTextField(i, 0.ToString());
            }
            Manager.Instance.SetTextField(13, cpuSum.ToString());
            Manager.Instance.SetTextField(6, playerSum.ToString());
            string winner = scoreComparer();
            Manager.Instance.SetTextField(15, winner);
            game_over = true;
        }

        if (((new_copy + n) == 13 || (new_copy + n) == 26) && game_over == false) //@D cpu turn check condiion again
        {
            random_CPU_input();
        }
        else if (game_over == false)
        {
            Manager.Instance.SetTextField(15, "User's Turn");
        }
    }

    public void random_CPU_input()
    { //@D random input
        int pitNumber = CPUturn() + 7;
        StartCoroutine(Delay_adder_cpu(pitNumber));
    }

    //Make CPU do random moves to test the game interface...
    //Make greedy cpu moves to test the game interface...
    public int CPUturn()
    {
        // private float miniMax(Node node, bool maxPlayer, int depth)
        Node presentNode = new Node();
        return (int)miniMax(presentNode, true, 0);

    }
}

   
