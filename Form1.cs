using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _8QueensApp
{
    public partial class Form1 : Form
    {
        private queenspuzzle live_puzzle;

        private List<PictureBox[]> board_pictureboxes;

        public Form1()
        {
            InitializeComponent();

            //Initialize our object that translates coordinates to the picturebox with the queen in it
            initializePictureboxBoard();

            live_puzzle = new queenspuzzle();
            updateSettingsGroupbox();
            updateBoard();
            positionSourceTextbox.Text = "Program launched. Algorithm has not started.";

            WindowState = FormWindowState.Maximized; //maximize our form window

        }     

        //For when we want to clear the session data groupbox, and the 4 listboxes.
        //This is called by the shuffle and reset buttons.
        public void clearProgess()
        {
            parent1listbox.Items.Clear();
            parent1listbox.Items.Add("Parent data will populate here.");

            parent2listbox.Items.Clear();
            parent2listbox.Items.Add("Parent data will populate here.");

            childlistbox.Items.Clear();
            childlistbox.Items.Add("Child data will populate here.");

            child_b_listbox.Items.Clear();
            child_b_listbox.Items.Add("Child data will populate here.");

            dropdownListbox.Items.Clear();
            dropdownListbox.Items.Add("Data from the down-down");
            dropdownListbox.Items.Add("will populate here.");

            //Clear everything in the session data groupbox
            clearSessionDataGroupbox();

            //Erase all puzzle progress so far
            live_puzzle.new_puzzle();
        }

        private void iterateFromDropdownButton_Click(object sender, EventArgs e)
        {
            if (iterateCountCombobox.SelectedItem == null)
                positionSourceTextbox.Text = "[Step failed] Select an amount to iterate from the drop down menu.";
            else
                iterate_algorithm(Int32.Parse(iterateCountCombobox.SelectedItem.ToString()));
        }

        private void iterateFromTextboxButton_Click(object sender, EventArgs e)
        {
            if (Int32.TryParse(iterateCountTextbox.Text, out int x))
                //textbox had a valid in value
                iterate_algorithm(x);
            else
                iterateCountTextbox.Text = "Invalid.";
        }

        private void iterate_algorithm(int iteration_count)
        {
            if(!live_puzzle.has_algorithm_started())
                setSessionDataInitialSettingsGroupbox();

            if(live_puzzle.has_algorithm_completed())
            {
                positionSourceTextbox.Text = "Algorithm has already completed.";
                return;
            }

            for (int i = 0; i < iteration_count; i++)
            {
                if (live_puzzle.algorithm_step())
                {
                    string turn_number = live_puzzle.get_iteration_count().ToString();
                    string child_number = live_puzzle.get_solution().individual_ID.ToString();
                    positionSourceTextbox.Text = "Child number " + child_number + " on turn " + turn_number + " was a solution.";
                    updateAfterAlgorithmStep(); //Update listboxes
                    updateBoard();
                    return;
                }
            }
            //A solution wasn't found.
            updateAfterAlgorithmStep(); //Update listboxes
        }

        private void updateTotalsGroupboxes()
        {
            totalsgroupboxParentstextbox.Text = live_puzzle.get_eligible_parents().ToString(); //Start of "totals this step"
            totalsFitnessOfParentsTextbox.Text = live_puzzle.get_avg_fitness_eligible_parents().ToString();
            sessiondataTotalsstepAcutalparentstextbox.Text = live_puzzle.get_actual_parents_count_current().ToString();
            sessionTotalsAvgfitness.Text = live_puzzle.get_avg_fitness_actual().ToString();
            totalsgroupboxChildrentextbox.Text = live_puzzle.get_children_count_current().ToString();
            totalsFitnessOfChildrenTextbox.Text = live_puzzle.get_children_fitness().ToString();
            totalsgroupboxMutationstextbox.Text = live_puzzle.get_mutation_count_current().ToString();
            finaltotalgroupboxParentstextbox.Text = live_puzzle.get_eligible_parents_total().ToString(); //Start of "totals"
            sessionTotalsAvgfitnesstextbox.Text = live_puzzle.get_avg_fitness_total().ToString();
            sessionTotalsActualparentsTextbox.Text = live_puzzle.get_actual_parents_count_total().ToString();
            finaltotalGroupboxChildrentextbox.Text = live_puzzle.get_children_total().ToString();
            finaltotalsgroupboxMutationstextbox.Text = live_puzzle.get_mutations_total().ToString();
            finaltotalsgroupboxIterationstextbox.Text = live_puzzle.get_iteration_count().ToString();
        }

        //This will populate the session data groupbox, and the listboxes.
        private void updateAfterAlgorithmStep()
        {
            updateTotalsGroupboxes(); //Update totals this step, and totals groupboxes
            int fitness = live_puzzle.get_best_child().fitness;
            string solution_as_string = String.Join(" ", live_puzzle.get_best_child().solution_data);
            positionSourceTextbox.Text =    "Generation " + live_puzzle.get_iteration_count().ToString() + 
                                            " produced this best specimen [" + solution_as_string + 
                                            "] with fitness " + fitness + "/28.";
            updateBoard();

            List<individual> temp_list;
            ListBox temp_listbox;
            string add_string;

            parent1listbox.Items.Clear();
            parent2listbox.Items.Clear();
            childlistbox.Items.Clear();
            child_b_listbox.Items.Clear();
            dropdownListbox.Items.Clear();
            dropdownListbox.Items.Add("Data from the drop-down");
            dropdownListbox.Items.Add("will populate here.");

            //Get our 4 lists we always handle.
            List<ListBox> listboxes = new List<ListBox>();
            listboxes.Add(parent1listbox);
            listboxes.Add(parent2listbox);
            listboxes.Add(childlistbox);
            listboxes.Add(child_b_listbox);

            List<List<individual>> population_lists = new List<List<individual>>();
            population_lists.Add(live_puzzle.get_a_parents());
            population_lists.Add(live_puzzle.get_b_parents());
            population_lists.Add(live_puzzle.get_a_children());
            population_lists.Add(live_puzzle.get_b_children());
            for(int i = 0; i < population_lists.Count; i++)
            {
                temp_list = population_lists[i];
                temp_listbox = listboxes[i];
                for (int j = 0; j < temp_list.Count; j++)
                {
                    add_string = "[ID: " + temp_list[j].individual_ID.ToString() + "] ";
                    add_string += string.Join(" ", temp_list[j].solution_data);
                    temp_listbox.Items.Add(add_string);
                }
            }
        }

        private void setSessionDataInitialSettingsGroupbox()
        {
            live_puzzle.assign_initial_settings();
            initpoptextbox.Text = live_puzzle.get_population_size_initial().ToString();
            sessiondataInitialsettingsFitnessimpactTextbox.Text = (live_puzzle.get_fitness_initial() * 100).ToString() + "%";
            crossovertextbox.Text = live_puzzle.get_crossover_position_initial().ToString();
            mutationtextbox.Text = (live_puzzle.get_mutation_initial() * 100).ToString() + "%";
        }

        private void clearSessionDataGroupbox()
        {
            clearInitialSettingsGroupbox();
            clearTotalsThisStepGroupbox();
            clearTotalsGroupbox();
        }

        private void clearInitialSettingsGroupbox()
        {
            initpoptextbox.Clear();
            sessiondataInitialsettingsFitnessimpactTextbox.Clear();
            crossovertextbox.Clear();
            mutationtextbox.Clear();
        }

        private void clearTotalsThisStepGroupbox()
        {
            totalsgroupboxChildrentextbox.Clear();
            totalsgroupboxParentstextbox.Clear();
            totalsFitnessOfParentsTextbox.Clear();
            sessionTotalsActualparentsTextbox.Clear();
            sessionTotalsAvgfitness.Clear();
            totalsFitnessOfChildrenTextbox.Clear();
            totalsgroupboxMutationstextbox.Clear();
        }

        private void clearTotalsGroupbox()
        {
            finaltotalGroupboxChildrentextbox.Clear();
            finaltotalgroupboxParentstextbox.Clear();
            finaltotalsgroupboxMutationstextbox.Clear();
            finaltotalsgroupboxIterationstextbox.Clear();
            sessionTotalsActualparentsTextbox.Clear();
            sessionTotalsAvgfitnesstextbox.Clear();
        }

        public void updateSettingsGroupbox()
        {
            SettingsPopulationSizeTextbox.Text = live_puzzle.get_population_size();
            SettingsFitnessTextbox.Text = live_puzzle.get_fitness();
            SettingsCrossoverTextbox.Text = live_puzzle.get_crossover_position().ToString();
            SettingsMutationTextbox.Text = live_puzzle.get_mutation();
        }

        public void clearBoard()
        {
            for (int i = 0; i < board_pictureboxes.Count; i++)
            {
                for (int j = 0; j < board_pictureboxes[i].Length; j++)
                {
                    board_pictureboxes[i][j].Image = null;
                }
            }
        }

        //updates the chessboard after the queenspuzzle object has assigned some member of the population to it, for display purposes
        public void updateBoard()
        {
            int[] board_to_display = live_puzzle.get_board().solution_data;
            int current_column = 0;

            clearBoard();

            for (int i = 0; i < 8; ++i)
            {
                //Reminder: [row][column]
                //Get the queen height stored at the ith position of our 2D board-array
                current_column = board_to_display[i];
                //i is the row
                board_pictureboxes[i][current_column].Image = Image.FromFile("queen.png");
            }
        }

        private void resetConfigurationButton_Click(object sender, EventArgs e)
        {
            updateSettingsGroupbox();
            initpopulationbox.Text = "Enter here";
            mutationtextbox.Text = "Enter here";
        }

        private void startOverButton_Click(object sender, EventArgs e)
        {
            //set default board should handle resetting the session progress for us
            live_puzzle.set_default_board();
            clearProgess();
            updateBoard();
        }

        private void initpopdropdownSet_Click(object sender, EventArgs e)
        {
            if (initpopdropdrown.SelectedItem == null)
            {
                positionSourceTextbox.Text = "[Configure failed] Select a population size from the drop-down menu.";
            }
            else
            {
                int to_set = Int32.Parse(initpopdropdrown.SelectedItem.ToString());
                live_puzzle.set_current_population(to_set);
                SettingsPopulationSizeTextbox.Text = to_set.ToString();
            }
        }

        private void setpopfromtext_Click(object sender, EventArgs e)
        {
            int x = 0;
            if (Int32.TryParse(initpopulationbox.Text, out x)) //this worked
            {
                if (x > 0)
                {
                    live_puzzle.set_current_population(x);
                    SettingsPopulationSizeTextbox.Text = x.ToString();
                }
                else
                    initpopulationbox.Text = "Invalid.";
            }
            else
                initpopulationbox.Text = "Invalid.";
        }

        private void setFitnessButton_Click(object sender, EventArgs e)
        {
            float x = fitnessTrackBar.Value / (float)2; //This makes index 0 = 0, index 1 = .5, and index 2 = 1.
            live_puzzle.set_current_fitness(x);
            x *= 100; //for percentage
            SettingsFitnessTextbox.Text = x.ToString() + "%";
        }

        private void setCrossoverPositionButton_Click(object sender, EventArgs e)
        {
            int x = crossoverbar.Value + 1;
            live_puzzle.set_current_crossover(x);
            SettingsCrossoverTextbox.Text = x.ToString();
        }

        private void setMutationFromDropdownButton_Click(object sender, EventArgs e)
        {
            if (configmutationCombobox.SelectedItem == null)
            {
                positionSourceTextbox.Text = "[Configure failed] Select a mutation percentage from the drop-down menu.";
            }
            else
            {
                float x = float.Parse(configmutationCombobox.SelectedItem.ToString());
                live_puzzle.set_current_mutation(x);
                x *= 100;
                SettingsMutationTextbox.Text = x.ToString() + "%";
            }
        }

        private void setMutationFromTextboxButton_Click(object sender, EventArgs e)
        {
            float x = 0;
            if (float.TryParse(configmutationTextbox.Text, out x)) //this worked
            {
                if (x >= 0 && x <= 1)
                {
                    live_puzzle.set_current_mutation(x);
                    x = x * 100;
                    SettingsMutationTextbox.Text = x.ToString() + "%";
                }
                else
                    configmutationTextbox.Text = "Invalid.";
            }
            else
                configmutationTextbox.Text = "Invalid.";
        }




        private void button9_Click(object sender, EventArgs e)
        {

        }
        private void a1_Click(object sender, EventArgs e)
        {

        }
        private void button13_Click(object sender, EventArgs e)
        {

        }
        private void button2_Click(object sender, EventArgs e)
        {

        }
        private void shuffleBoardButton_Click(object sender, EventArgs e)
        {

        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void label5_Click(object sender, EventArgs e)
        {

        }
        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }
        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox7_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox9_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox10_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox21_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox15_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox16_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox17_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox18_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox19_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox20_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }
        private void pictureBox61_Click(object sender, EventArgs e)
        {

        }
        private void a8_Click(object sender, EventArgs e)
        {

        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    
        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void initpopdropdrown_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public void initializePictureboxBoard()
        {
            board_pictureboxes = new List<PictureBox[]>();

            for (int i = 0; i < 8; i++)
            { board_pictureboxes.Add(new PictureBox[8]); }

            //[0][0] is the bottom left of the board
            //[row][column]
            board_pictureboxes[0][0] = a1;
            board_pictureboxes[0][1] = a2;
            board_pictureboxes[0][2] = a3;
            board_pictureboxes[0][3] = a4;
            board_pictureboxes[0][4] = a5;
            board_pictureboxes[0][5] = a6;
            board_pictureboxes[0][6] = a7;
            board_pictureboxes[0][7] = a8;
            board_pictureboxes[1][0] = b1;
            board_pictureboxes[1][1] = b2;
            board_pictureboxes[1][2] = b3;
            board_pictureboxes[1][3] = b4;
            board_pictureboxes[1][4] = b5;
            board_pictureboxes[1][5] = b6;
            board_pictureboxes[1][6] = b7;
            board_pictureboxes[1][7] = b8;
            board_pictureboxes[2][0] = c1;
            board_pictureboxes[2][1] = c2;
            board_pictureboxes[2][2] = c3;
            board_pictureboxes[2][3] = c4;
            board_pictureboxes[2][4] = c5;
            board_pictureboxes[2][5] = c6;
            board_pictureboxes[2][6] = c7;
            board_pictureboxes[2][7] = c8;
            board_pictureboxes[3][0] = d1;
            board_pictureboxes[3][1] = d2;
            board_pictureboxes[3][2] = d3;
            board_pictureboxes[3][3] = d4;
            board_pictureboxes[3][4] = d5;
            board_pictureboxes[3][5] = d6;
            board_pictureboxes[3][6] = d7;
            board_pictureboxes[3][7] = d8;
            board_pictureboxes[4][0] = e1;
            board_pictureboxes[4][1] = e2;
            board_pictureboxes[4][2] = e3;
            board_pictureboxes[4][3] = e4;
            board_pictureboxes[4][4] = e5;
            board_pictureboxes[4][5] = e6;
            board_pictureboxes[4][6] = e7;
            board_pictureboxes[4][7] = e8;
            board_pictureboxes[5][0] = f1;
            board_pictureboxes[5][1] = f2;
            board_pictureboxes[5][2] = f3;
            board_pictureboxes[5][3] = f4;
            board_pictureboxes[5][4] = f5;
            board_pictureboxes[5][5] = f6;
            board_pictureboxes[5][6] = f7;
            board_pictureboxes[5][7] = f8;
            board_pictureboxes[6][0] = g1;
            board_pictureboxes[6][1] = g2;
            board_pictureboxes[6][2] = g3;
            board_pictureboxes[6][3] = g4;
            board_pictureboxes[6][4] = g5;
            board_pictureboxes[6][5] = g6;
            board_pictureboxes[6][6] = g7;
            board_pictureboxes[6][7] = g8;
            board_pictureboxes[7][0] = h1;
            board_pictureboxes[7][1] = h2;
            board_pictureboxes[7][2] = h3;
            board_pictureboxes[7][3] = h4;
            board_pictureboxes[7][4] = h5;
            board_pictureboxes[7][5] = h6;
            board_pictureboxes[7][6] = h7;
            board_pictureboxes[7][7] = h8;
        }
        private void applyAllConfigSettingsButton_Click(object sender, EventArgs e) { }
        private void indiatePositionSourceTextbox_TextChanged(object sender, EventArgs e) { }

        private void viewParent1Button_Click(object sender, EventArgs e)
        {
            if (parent1listbox.SelectedItem != null)
            {
                int index_of_bracket = parent1listbox.SelectedItem.ToString().IndexOf(']');
                string split_string = parent1listbox.SelectedItem.ToString().Substring(index_of_bracket + 2, 15); //Ugly, but gets the ID part off.
                view_individual(split_string.Split(' ').Select(n => Convert.ToInt32(n)).ToArray(), "Parent 1");
            }
        }

        private void viewParent2Button_Click(object sender, EventArgs e)
        {
            if (parent2listbox.SelectedItem != null)
            {
                int index_of_bracket = parent2listbox.SelectedItem.ToString().IndexOf(']');
                int length = parent2listbox.SelectedItem.ToString().Length;
                string split_string = parent2listbox.SelectedItem.ToString().Substring(index_of_bracket + 2, 15); //Ugly, but gets the ID part off.
                view_individual(split_string.Split(' ').Select(n => Convert.ToInt32(n)).ToArray(), "Parent 2");
            }
        }

        private void viewChildButton_Click(object sender, EventArgs e)
        {
            if (childlistbox.SelectedItem != null)
            {
                int index_of_bracket = childlistbox.SelectedItem.ToString().IndexOf(']');
                int length = childlistbox.SelectedItem.ToString().Length;
                string split_string = childlistbox.SelectedItem.ToString().Substring(index_of_bracket + 2, 15); //Ugly, but gets the ID part off.
                view_individual(split_string.Split(' ').Select(n => Convert.ToInt32(n)).ToArray(), "Child 1");
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (child_b_listbox.SelectedItem != null)
            {
                int index_of_bracket = child_b_listbox.SelectedItem.ToString().IndexOf(']');
                int length = child_b_listbox.SelectedItem.ToString().Length;
                string split_string = child_b_listbox.SelectedItem.ToString().Substring(index_of_bracket + 2, 15); //Ugly, but gets the ID part off.
                view_individual(split_string.Split(' ').Select(n => Convert.ToInt32(n)).ToArray(), "Child 2");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dropdownListbox.SelectedItem != null && live_puzzle.has_algorithm_started() == true)
            {
                int index_of_bracket = dropdownListbox.SelectedItem.ToString().IndexOf(']');
                int length = dropdownListbox.SelectedItem.ToString().Length;
                string split_string = dropdownListbox.SelectedItem.ToString().Substring(index_of_bracket + 2, 15); //Ugly, but gets the ID part off.
                view_individual(split_string.Split(' ').Select(n => Convert.ToInt32(n)).ToArray(), iterationdataDropdrown.SelectedItem.ToString());
            }

        }

        private void view_individual(int[] board_config_to_view, string list_source)
        {
            individual to_display = new individual(board_config_to_view);
            live_puzzle.set_board(to_display);
            updateBoard();
            string fitness = live_puzzle.get_board().fitness.ToString();
            positionSourceTextbox.Text = "Individual from '" + list_source + "' has fitness " +  fitness + "/28.";
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!live_puzzle.has_algorithm_started())
                return;
            List<individual> temp_list = null; 

            dropdownListbox.Items.Clear();
            string selected_item = iterationdataDropdrown.SelectedItem.ToString();
            string add_string;

            if (selected_item == "Eligible parents")
                temp_list = live_puzzle.get_parents();
            if (selected_item == "Actual parents")
                temp_list = live_puzzle.get_actual_parents();
            if (selected_item == "Children")
                temp_list = live_puzzle.get_children();
            if (selected_item == "Mutations")
                temp_list = live_puzzle.get_mutations();        

            for (int j = 0; j < temp_list.Count; j++)
            {
                add_string = "[ID: " + temp_list[j].individual_ID.ToString() + "] ";
                add_string += string.Join(" ", temp_list[j].solution_data);
                dropdownListbox.Items.Add(add_string);
            }
        }

        private void runIndefinitelyButton_Click(object sender, EventArgs e)
        {
            while (live_puzzle.has_algorithm_completed() != true)
            { iterate_algorithm(1); }
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {

        }
    }
}
