using Unity.Collections;
using static Unity.Mathematics.math;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;

namespace BioCrowdsTechDemo
{
    /// <summary>
    /// Static Class for BioCrowds agent helper functions.
    /// </summary>
    public static class AgentFunctions
    {
        // weight of a sample given a position and goal
        public static float F(float2 sample, float2 position, float2 goal)
        {
            var agent_sample = sample - position;
            float y = length(agent_sample);

            if (y < 0.000f) return 0.0f;

            float x = 1f;
            float d = dot(agent_sample, normalize(goal - position));

            return (1.0f + (d / (x + y))) / (1.0f + y);
        }

        
        
    }
}