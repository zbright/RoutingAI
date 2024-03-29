﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingAI.Algorithms.PlanetColonizer.Interfaces
{
    /// <summary>
    /// Represents a group of individuals in a genetic algorithm
    /// </summary>
    /// <typeparam name="TIndividualType">Type of individual</typeparam>
    public abstract class Population<TIndividualType> where TIndividualType : Individual
    {
        protected int BestFitness = Int32.MaxValue;  // Fitness value of the best individual
        protected TIndividualType bestIndividual = default(TIndividualType); // Best individual
        protected int CataclysmCountdown; 
        protected int CurIteration = 0;
        protected TIndividualType[] Individuals;
        protected int InitialCataclysmCountdown = 1000;
        protected int InitialMutationRate = 70;
        protected int MaxIterationsWithoutImprovements;
        protected int MutationRate;
        public int NbIterationsWithoutImprovements = 0;
        protected int _populationSize = 10;
        protected Random Rand = new Random();

        public virtual void Optimize()
        {
            NbIterationsWithoutImprovements++;
            CurIteration++;

            ComputeCrossovers();
            ComputeMutations();
            ComputeCataclysms();

            bestIndividual.Optimize();
        }

        protected virtual void ComputeCrossovers()
        {
            if (_populationSize == 1)
            {
                return;
            }

            TIndividualType parent;
            int nbChildren = 0;
            int maxChildren = ComputeMaxChildren();

            while (nbChildren < maxChildren && (parent = SelectHealthyIndividual()) != null)
            {
                if (Crossover(parent))
                {
                    nbChildren++;
                }
            }
        }

        protected virtual bool Crossover(TIndividualType parent)
        {
            TIndividualType p1, p2, child;
            SelectCrossoverIndividuals(out p1, out p2, out child);

            child.Crossover(p1, p2);

            if (child.Fitness < BestFitness)
            {
                OnNewBestIndividual(child);
            }

            return true;
        }

        protected bool CanWeakDieByCrossover(TIndividualType weak, TIndividualType parent)
        {
            return weak.ID != parent.ID;
        }

        protected virtual void OnNewBestIndividual(TIndividualType individual)
        {
            // really a new best?
            if (individual.Fitness < BestFitness)
            {
                MutationRate = InitialMutationRate;
                CataclysmCountdown = InitialCataclysmCountdown;
                NbIterationsWithoutImprovements = 0;
            }

            bestIndividual = individual;
            BestFitness = individual.Fitness;
        }

        protected virtual void ComputeMutations()
        {
            if (_populationSize == 1)
            {
                return;
            }

            int maxMutations = ComputeMaxMutations();

            for (int t = 0; t < maxMutations; t++)
            {
                if (Rand.Next(MutationRate) == 0)
                {
                    SelectWeakIndividual().Mutate();
                }
            }

            MutationRate--;

            if (MutationRate < 1)
            {
                MutationRate = 1;
            }
        }


        protected virtual int ComputeMaxChildren()
        {
            return (int)(_populationSize * 0.03 + 2);
        }

        protected virtual int ComputeMaxMutations()
        {
            return (int)(_populationSize * 0.03 + 2);
        }


        protected virtual void ComputeCataclysms()
        {
            if (IsCataclysmTime())
            {
                // boom! regenerate new distributions...
                RegeneratePopulation();

                CataclysmCountdown = InitialCataclysmCountdown;
            }

            CataclysmCountdown--;
        }


        protected bool IsCataclysmTime()
        {
            if (_populationSize == 1)
            {
                return false;
            }

            return CataclysmCountdown <= 0;
        }

        protected abstract void RegeneratePopulation();


        protected virtual TIndividualType SelectWeakIndividual()
        {
            int maxFitness = Int32.MinValue;
            TIndividualType weakestIndividual = default(TIndividualType);

            foreach (TIndividualType individual in Individuals)
            {
                if (individual != null && individual.Fitness > maxFitness && individual.ID != bestIndividual.ID)
                {
                    weakestIndividual = individual;
                    maxFitness = individual.Fitness;
                }
            }

            return weakestIndividual;
        }

        protected virtual void SelectCrossoverIndividuals(out TIndividualType p1, out TIndividualType p2,
                                                          out TIndividualType child)
        {
            p1 = Individuals[Rand.Next() % _populationSize];
            p2 = Individuals[Rand.Next() % _populationSize];
            child = SelectWeakIndividual();

            if (p1.ID == p2.ID || p1.ID == child.ID || p2.ID == child.ID)
            {
                SelectCrossoverIndividuals(out p1, out p2, out child);
            }
        }

        protected virtual TIndividualType SelectHealthyIndividual()
        {
            int curIndex = Rand.Next(_populationSize);
            for (int t = 0; t < _populationSize; t++)
            {
                TIndividualType individual = Individuals[curIndex % _populationSize];

                if (individual != null)
                {
                    if (individual.ID != bestIndividual.ID)
                    {
                        int odds = (individual.Fitness * 3 / (BestFitness)) + 1;

                        int n = Rand.Next(odds);

                        if (n == 0)
                        {
                            return individual;
                        }
                    }
                    curIndex++;
                }
            }

            return default(TIndividualType);
        }
    }
}
