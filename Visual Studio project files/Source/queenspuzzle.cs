using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8QueensApp
{
    class queenspuzzle
    {
        //The board is just a 2d array, as queens never overlap on a column.
        //Each position stores a vertical index for the queen on that column.
        //A value of 0 at index i means the queen is on the bottom row, to match the board.
        //This board is used for display purposes. After a control has been used that generates a board to be displayed in the windows form,
        //We can pull the board using a get_board function.
        private individual board;

        private List<individual> parents;
        private List<individual> a_parents; //a and b parents are for forming parent pairs
        private List<individual> b_parents;
        private List<individual> a_children; //used for the listbox displays
        private List<individual> b_children;
        private List<individual> children;
        private List<individual> mutations;
        private List<individual> actual_parents;

        //Session data - Initial settings groupbox variables
        private int population_size_initial;
        private int [] crossover_position_initial;
        private float mutation_setting_initial;

        //Settings groupbox variables (top left)
        private int population_size_current;
        private int [] crossover_position_current;
        private float mutation_setting_current;

        //Session data - totals this step groupbox variables
        //Eligible parents is taken from population size_current
        private float avg_fitness_eligible_parents_current;
        //actual_parents list is used for parent count
        private float avg_fitness_actual_parents_current;
        //Children count is obtained from the children list
        private float avg_fitness_children_current;
        private int mutation_count_current;

        //Session data - totals groupbox variables
        private int eligible_parents_total;
        private float avg_fitness_increase;
        private int actual_parents_total;
        private int iterations;
        private int children_count_total;
        private int mutation_count_total;
        
        private int size_of_probability_array;

        //For picking random parents
        List<individual> random_num_to_individual;

        //consistent seed
        private Random random;

        //This will store our solution
        private individual solution;

        public queenspuzzle()
        {
            //For parent selecting
            size_of_probability_array = 10000;

            //Start the program with the queens at the bottom of the board.
            set_default_board();
            setInitial();
        }

        public bool algorithm_step()
        {            
            prepare_to_step(); 
            build_probability_list();
            return breed_children(); //returns true if a solution is found
        }

        private void prepare_to_step()
        {
            if (iterations < 1) //first step
            {
                //Generate some random boards, since this is the first step.
                random = new Random();
                parents = new List<individual>();
                for (int i = 0; i < population_size_current; i++)
                {
                    new_puzzle(); //Erase all previous totals.
                    set_initial_data_from_current(); //Populate our initial data, strictly for historical and display purposes
                    parents.Add(new individual(i, getShuffledBoard())); //i is set as the ID, which just needs to be a unique identifier amongst peers.
                }
            }
            else //Convert stored children list in parent list now
                parents = children;

            //Session data - Totals this step groupbox
            avg_fitness_eligible_parents_current = calculate_avg_fitness(parents); //avg fitness of all parents
            //Eligible parents is population value
            avg_fitness_actual_parents_current = 0;
            //Children count is children.count
            avg_fitness_children_current = 0;
            mutation_count_current = 0;

            //Our lists for displaying in the listboxes
            a_parents = new List<individual>();
            b_parents = new List<individual>();
            a_children = new List<individual>();
            b_children = new List<individual>();
            actual_parents = new List<individual>();
            children = new List<individual>();
            mutations = new List<individual>();
            rank_parents_by_fitness(); //sort parents list
        }

        private void build_probability_list()
        {
            int total_fitness = 0;
            int fitness_int = 0;
            int parent_index = 0;

            //pointers to individuals
            //this could be a terrible way to solve this problem
            
            //We'll take fitness percentage and store a number of these 10000 pointers according to individual fitness/total fitness

            random_num_to_individual = new List<individual>();

            //Get the total fitness of all parents to assess how fit the individual parent is.
            for (int i = 0; i < parents.Count; i++)
            {
                total_fitness += parents[i].fitness;
            }

            //Fill in our array with parents based on probability
            while(random_num_to_individual.Count < size_of_probability_array)
            {
                //Get this parent's fitness, proportional to the combined fitness of all parents. Add one for rounding.
                fitness_int = ((parents[parent_index].fitness * size_of_probability_array) / total_fitness) + 1;
                while(fitness_int > 0)
                {
                    random_num_to_individual.Add(parents[parent_index]);
                    --fitness_int;
                    if (random_num_to_individual.Count == size_of_probability_array)
                        fitness_int = 0;
                }

                ++parent_index;
            }            
        }

        private bool breed_children()
        {
            //We'll be randomly picking parents based on their fitness.
            //We'll pick a random number between 10,000 for each parent selection.
            int array_index;
            individual parent_a;
            individual parent_b;
            individual child_a;
            individual child_b = null;
            int crossover_position;

            bool keep_looping = true;

            List<individual> temp_random_num_list;

            //this is used because we create two children at a time, but only add one at a time.
            bool add_sibling = false;            

            while(children.Count < population_size_current && keep_looping)
            {
                if (add_sibling == false)
                { 
                    array_index = random.Next(0, random_num_to_individual.Count); //Pick the first parent
                    parent_a = random_num_to_individual[array_index];

                    //Get parent b.
                    temp_random_num_list = new List<individual>(random_num_to_individual); //Copy our list of parents so we can remove parent a from it
                    temp_random_num_list.RemoveAll(individual => individual.individual_ID == parent_a.individual_ID);
                    array_index = random.Next(0, temp_random_num_list.Count);
                    parent_b = temp_random_num_list[array_index];

                    actual_parents.Add(parent_a);
                    actual_parents.Add(parent_b);
                    a_parents.Add(parent_a);
                    b_parents.Add(parent_b);

                    //We have our parents. Generate our children.
                    child_a = new individual(parent_a); //Give the child parent a's genetic information, which will be overwritten at some part by parent b.
                    child_a.individual_ID = children.Count;

                    child_b = new individual(parent_b);
                    child_b.individual_ID = children.Count + 1; //+1 because we dont want to increase children count yet.

                    crossover_position = random.Next(crossover_position_current[0], crossover_position_current[1] + 1);

                    //Genetic crossover
                    Array.Copy( parent_b.solution_data,
                                crossover_position, 
                                child_a.solution_data,
                                crossover_position, 
                                8 - crossover_position);

                    Array.Copy( parent_a.solution_data,
                                crossover_position,
                                child_b.solution_data,
                                crossover_position,
                                8 - crossover_position);

                    child_a.assess_fitness();
                    child_b.assess_fitness();

                    children.Add(child_a);
                    a_children.Add(child_a);
                    if (child_a.fitness == 28)
                    { //End the loop due to a solution being found
                        keep_looping = false;
                        solution = child_a;
                    }
                    add_sibling = true;
                }
                else
                {
                    //add previous remaining sibling instead of generating a new sibling pair.
                    children.Add(child_b);
                    b_children.Add(child_b);
                    if (child_b.fitness == 28)
                    {   //End the loop due to a solution being found
                        keep_looping = false;
                        solution = child_b;
                    }
                    add_sibling = false;   
                }
            } //We now have children.
            updateAllTotals();

            //Sort children
            children = (from s in children orderby s.fitness descending select s).ToList();
            board = children[0];

            //If keep_looping is false, a solution was found.
            if (!keep_looping)
                return true;

            return false; //return false indicates no solution 
        }       

        private void updateAllTotals()
        {
            //This section is "session data - Totals this step"
            actual_parents = actual_parents.Distinct().ToList(); //remove duplicates
            avg_fitness_actual_parents_current = calculate_avg_fitness(actual_parents);
            avg_fitness_children_current = calculate_avg_fitness(children);

            //This seciton is Session Data - Totals
            eligible_parents_total += population_size_current;

            //To get the fitness increase, we have to un-average this number, then re-average it.
            avg_fitness_increase *= iterations; //It was averaged with the current iteration count, so multiply by that to get the pre-division quantity.

            avg_fitness_increase += avg_fitness_children_current - avg_fitness_eligible_parents_current; //Add this increase to the average totals

            ++iterations; //Now average again
            avg_fitness_increase = avg_fitness_increase / iterations;
            actual_parents_total += actual_parents.Count;
            children_count_total += children.Count;
            mutation_count_total += mutation_count_current;
        }

        public void new_puzzle()
        {
            //erase all totals
            eligible_parents_total = 0;
            avg_fitness_increase = 0;
            actual_parents_total = 0;
            iterations = 0;
            children_count_total = 0;
            mutation_count_total = 0;
            solution = null;
        }



        public int[] getShuffledBoard()
        {
            int[] to_return = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 8; i++)
            {
                to_return[i] = random.Next(0, 8);
            }

            if(individual.assess_fitness(to_return) == 28)
            //This puzzle was a solution, so we dont want it. That defeats the point of the algorithm.
                return getShuffledBoard();
            return to_return;
        }

        private float calculate_avg_fitness(List<individual> calculate_from)
        {
            float x = 0;
            for (int i = 0; i < calculate_from.Count; i++)
            {
                x += calculate_from[i].fitness;
            }

            return x / calculate_from.Count;
        }

        public void assign_initial_settings()
        {
            population_size_initial = population_size_current;
            crossover_position_initial = crossover_position_current;
            mutation_setting_initial = mutation_setting_current;
        }
        //Session data - Initial data groupbox
        private void set_initial_data_from_current()
        {
            population_size_initial = population_size_current;
            crossover_position_initial = crossover_position_current;
            mutation_setting_initial = mutation_setting_current;
        }
        public List<individual> get_individual_lists(int selector)
        {
            //1 = parent1, 2 = parent 2, 3 = child 1, 4 = child 2
            if (selector == 1)
                return a_parents;
            if (selector == 2)
                return b_parents;
            if (selector == 3)
                return a_children;
            if (selector == 4)
                return b_children;
            if (selector == 5)
                return parents;
            if (selector == 6)
                return actual_parents;
            if (selector == 7)
                return children;
            if (selector == 8)
                return mutations;
            else return null;
        }
        public void setInitial()
        {
            population_size_current = 10000;
            crossover_position_current = new int[2] { 1, 6 };
            mutation_setting_current = 0;
        }
        public void set_board(int [] to_set)
        {
            board = new individual(to_set);
        }
        public void set_board(individual to_set)
        {
            board = to_set;
        }
        public bool has_algorithm_completed()
        {
            if (solution != null)
                return true;
            return false;
        }
        public string get_mutation()
        {
            if (mutation_setting_current == 0)
                return "0%";
            return (mutation_setting_current * 100).ToString() + "%";
        }
        public string get_crossover_position() { return crossover_position_current[0].ToString() + ", " + crossover_position_current[1].ToString(); }
        private void rank_parents_by_fitness() { parents = (from s in parents orderby s.fitness descending select s).ToList(); }
        public bool has_algorithm_started()
        {
            if (iterations > 0)
                return true;
            return false;
        }
        public List<individual> get_eligible_parents() { return parents; }
        public string get_population_size() { return population_size_current.ToString(); }
        public int get_population_size_initial() { return population_size_initial; }
        public string get_crossover_position_initial() { return crossover_position_initial[0].ToString() + ", " + crossover_position_initial[1].ToString(); }
        public float get_mutation_initial() { return mutation_setting_initial; }
        public int get_eligible_parent_parent_current() { return population_size_current; }
        public float get_avg_fitness_eligible_parents() { return avg_fitness_eligible_parents_current; }
        public int get_actual_parents_count_current() { return actual_parents.Count; }
        public float get_avg_fitness_actual() { return avg_fitness_actual_parents_current; }
        public int get_children_count_current() { return children.Count; }
        public individual get_solution() { return solution; }
        public float get_children_fitness() { return avg_fitness_children_current; }
        public int get_iteration_count() { return iterations; }
        public int get_mutation_count_current() { return mutations.Count; }
        public int get_eligible_parents_total() { return eligible_parents_total; }
        public float get_avg_fitness_total() { return avg_fitness_increase; }
        public int get_actual_parents_count_total() { return actual_parents_total; }
        public int get_children_total() { return children_count_total; }
        public int get_mutations_total() { return mutation_count_total; }
        public void set_default_board() { board = new individual(); }
        public void set_current_population(int to_set) { population_size_current = to_set; }
        public void set_current_crossover(int [] to_set) { crossover_position_current = to_set; }
        public void set_current_mutation(float to_set) { mutation_setting_current = to_set; }
        public individual get_best_child() { return children[0];}
        public individual get_board() { return board; }
        public List<individual> get_a_parents() { return a_parents; }
        public List<individual> get_b_parents() { return b_parents; }
        public List<individual> get_a_children() { return a_children; }
        public List<individual> get_b_children() { return b_children; }
        public List<individual> get_mutations() { return mutations; }
        public List<individual> get_parents() { return parents; }
        public List<individual> get_actual_parents() { return parents; }
        public List<individual> get_children() { return children; }
        
    }
}
