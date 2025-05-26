using System;
using System.Linq;
using System.Collections.Generic;

public class AliasMethodRandomizer {
	private readonly int[] alias;
	private readonly float[] probability;
	private readonly Random random;

	// O(n) time complexity, but only happens once upon initialization. Only need to re-initialize if weights (input probabilities) change.
	public AliasMethodRandomizer(float[] inputProbabilities) {
		if (inputProbabilities == null || inputProbabilities.Length == 0) {
			throw new ArgumentException("Input probabilities array cannot be null or empty.", nameof(inputProbabilities));
		}

		random = new Random(Guid.NewGuid().GetHashCode());

		probability = new float[inputProbabilities.Length];
		alias = new int[inputProbabilities.Length];

		float average = 1.0f / inputProbabilities.Length;

		float[] probabilities = new float[inputProbabilities.Length];

		for (int i = 0; i < probabilities.Length; i++) {
			probabilities[i] = inputProbabilities[i];
		}

		Stack<int> small = [];
		Stack<int> large = [];

		for (int i = 0; i < probabilities.Length; i++) {
			if (probabilities[i] >= average) {
				large.Push(i);
			} else {
				small.Push(i);
			}
		}

		while (small.Count != 0 && large.Count != 0) {
			int less = small.Pop();
			int more = large.Pop();

			probability[less] = probabilities[less] * probabilities.Length;
			alias[less] = more;

			probabilities[more] = (probabilities[more] + probabilities[less]) - average;

			if (probabilities[more] >= 1.0f / probabilities.Length) {
				large.Push(more);
			} else {
				small.Push(more);
			}
		}

		while (small.Count != 0) {
			probability[small.Pop()] = 1.0f;
		}

		while (large.Count != 0) {
			probability[large.Pop()] = 1.0f;
		}
	}

	public AliasMethodRandomizer(int[] weights) : this(convertWeightsToProbabilities((weights))) {}

	// O(1) time complexity.
	public int next() {
		int column = random.Next(0, probability.Length);
		bool coinToss = random.NextDouble() < probability[column];

		return coinToss ? column : alias[column];
	}

	private static float[] convertWeightsToProbabilities(int[] weights) {
		if (weights == null || weights.Length == 0) {
			throw new ArgumentException("Weights array cannot be null or empty.", nameof(weights));
		}

		float totalWeight = weights.Sum();
		if (totalWeight <= 0) {
			throw new ArgumentException("The sum of weights must be greater than zero.", nameof(weights));
		}

		float[] probabilities = new float[weights.Length];
		for (int i = 0; i < probabilities.Length; i++) {
			if (weights[i] < 0) {
				throw new ArgumentException("Individual weights cannot be negative.", nameof(weights));
			}
			probabilities[i] = weights[i] / totalWeight;
		}

		return probabilities;
	}
}
