using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _8QueensApp
{
    //This is the object for managing 8-queens puzzle processes.
    class individual
    {
        public int[] solution_data;
        public int individual_ID;
        public int fitness;
        public string solution_string;

        public individual()
        {
            solution_data = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public individual(int[] to_set)
        {
            solution_data = to_set;
            assess_fitness();
            solution_string = string.Join(" ", solution_data);
            individual_ID = -1; //This board should only exist because it was created from the listboxes. -1 ID will show this.
        }

        public individual(individual set_from)
        {
            individual_ID = set_from.individual_ID;
            solution_data = new int[set_from.solution_data.Length];
            set_from.solution_data.CopyTo(solution_data, 0);
            assess_fitness();
            solution_string = string.Join(" ", solution_data);
        }

        public individual(int set_id, int[] set_data)
        {
            individual_ID = set_id;
            solution_data = set_data;
            assess_fitness();
            solution_string = string.Join(" ", solution_data);
        }

        public void assess_fitness()
        {
            //Count how many unique pairs of queens are not attacking one another.
            //i < 7 here because we dont iterate on the 8th slot
            fitness = 0;

            for (int i = 0; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    //If the values of the array are equal, these queens attack horizontally.
                    //If they're x apart in the array, and array values are also x apart, they are diagonal.
                    if (solution_data[i] == solution_data[j] || Math.Abs(i - j) == Math.Abs(solution_data[i] - solution_data[j]))
                        ++fitness;
                }
                //If all 8 queens are on a line, there will be 28 attacking queen pairs.
            }
            fitness = 28 - fitness;
        }

        public static int assess_fitness(int [] to_assess)
        {
            int attack_count = 0;

            //Count how many unique pairs of queens are not attacking one another.
            //i < 7 here because we dont iterate on the 8th slot
            for (int i = 0; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    //If the values of the array are equal, these queens attack horizontally.
                    //If they're x apart in the array, and array values are also x apart, they are diagonal.
                    if (to_assess[i] == to_assess[j] || Math.Abs(i - j) == Math.Abs(to_assess[i] - to_assess[j]))
                        ++attack_count;
                }
                //If all 8 queens are on a line, there will be 28 attacking queen pairs.
            }
            return 28 - attack_count;
        }

        public bool mutate(float mutation_percent, Random random)
        {
            int float_times_one_hundred = (int)(mutation_percent * 100);
            int mutate_index = random.Next(0, 8);
            int mutate_into = random.Next(0, 8);
            if (mutation_percent > 0)
                if (random.Next(1, 100 + 1) <= float_times_one_hundred)
                {
                    solution_data[mutate_index] = mutate_into;
                    return true;
                }
            return false;
        }
    }
}
