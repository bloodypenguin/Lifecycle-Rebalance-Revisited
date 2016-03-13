namespace WG_CitizenEdit
{
    public class DataStore
    {
        // Citizen life span --------------------------------------------------
        public static int lifeSpanMultiplier = 4;

        // Travel -------------------------------------------------------------
        public const int LOW = 0;
        public const int HIGH = 1;

        // Array indexes
        public const int CAR = 0;
        public const int BIKE = 1;
        public const int TAXI = 2;

        // TODO? - Percentage for bike riding (add bike)
        public static int bikeIncrease = 10;
        // TODO? - Percentage for free public transport (subtract car)

        // Cache items with lowest values
        public static uint citizenCache = 0u;
        public static int[] cacheArray;
        public static bool livesInBike = false;

        // wealth, home building density, age, transportmode
        public static int[][][] wealth_low = { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {10, 30, 0},
                                                             new int [] {50, 20, 1},
                                                             new int [] {65, 10, 2},
                                                             new int [] {35,  0, 3} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {2, 30, 0},
                                                             new int [] {3, 20, 1},
                                                             new int [] {4, 10, 2},
                                                             new int [] {3,  0, 3} }};

        public static int[][][] wealth_med = { new int[][] { new int [] { 0, 40, 0},
                                                             new int [] {12, 30, 1},
                                                             new int [] {55, 20, 2},
                                                             new int [] {70, 10, 4},
                                                             new int [] {40,  0, 6} },

                                               new int[][] { new int [] {0, 40, 0},
                                                             new int [] {3, 30, 1},
                                                             new int [] {5, 20, 2},
                                                             new int [] {6, 10, 3},
                                                             new int [] {5,  0, 5} }};

        public static int[][][] wealth_high = { new int[][] { new int [] { 0, 40, 0},
                                                              new int [] {15, 30, 2},
                                                              new int [] {60, 20, 3},
                                                              new int [] {75, 10, 4},
                                                              new int [] {50,  0, 6} },

                                                new int[][] { new int [] { 0, 40, 0},
                                                              new int [] { 4, 30, 2},
                                                              new int [] { 6, 20, 3},
                                                              new int [] { 8, 10, 4},
                                                              new int [] { 7,  0, 5} }};
    }
}