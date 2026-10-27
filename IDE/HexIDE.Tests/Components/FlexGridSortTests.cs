using HexIDE.Runtime.Components.InBox;

namespace HexIDE.Tests.Components;

public class FlexGridSortTests
{
    // VB6 SP6 native/p-code recordings: #1099, issuecomment-6438162882, run 1 (sort6.txt).
    public static TheoryData<int, int, int, string> Orders => new()
    {
        { 1, 2, 1, "k1 k2" },
        { 1, 2, 2, "k1 k2" },
        { 1, 2, 3, "k1 k2" },
        { 1, 2, 4, "k1 k2" },
        { 1, 2, 5, "k1 k2" },
        { 1, 2, 6, "k1 k2" },
        { 1, 2, 7, "k1 k2" },
        { 1, 2, 8, "k1 k2" },
        { 1, 3, 1, "k1 k2 k3" },
        { 1, 3, 2, "k1 k2 k3" },
        { 1, 3, 3, "k1 k2 k3" },
        { 1, 3, 4, "k1 k2 k3" },
        { 1, 3, 5, "k1 k2 k3" },
        { 1, 3, 6, "k1 k2 k3" },
        { 1, 3, 7, "k1 k2 k3" },
        { 1, 3, 8, "k1 k2 k3" },
        { 1, 5, 1, "k1 k2 k3 k4 k5" },
        { 1, 5, 2, "k1 k2 k3 k4 k5" },
        { 1, 5, 3, "k1 k2 k3 k4 k5" },
        { 1, 5, 4, "k1 k2 k3 k4 k5" },
        { 1, 5, 5, "k1 k2 k3 k4 k5" },
        { 1, 5, 6, "k1 k2 k3 k4 k5" },
        { 1, 5, 7, "k1 k2 k3 k4 k5" },
        { 1, 5, 8, "k1 k2 k3 k4 k5" },
        { 1, 8, 1, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 2, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 3, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 4, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 5, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 6, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 7, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 8, 8, "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 1, 17, 1, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 2, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 3, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 4, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 5, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 6, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 7, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 1, 17, 8, "k1 k2 k3 k4 k5 k6 k7 k8 k9 k10 k11 k12 k13 k14 k15 k16 k17" },
        { 2, 2, 1, "a1 b2" },
        { 2, 2, 2, "b2 a1" },
        { 2, 2, 3, "a1 b2" },
        { 2, 2, 4, "a1 b2" },
        { 2, 2, 5, "a1 b2" },
        { 2, 2, 6, "b2 a1" },
        { 2, 2, 7, "a1 b2" },
        { 2, 2, 8, "b2 a1" },
        { 2, 3, 1, "a1 a3 b2" },
        { 2, 3, 2, "b2 a1 a3" },
        { 2, 3, 3, "a1 b2 a3" },
        { 2, 3, 4, "a1 b2 a3" },
        { 2, 3, 5, "a1 a3 b2" },
        { 2, 3, 6, "b2 a1 a3" },
        { 2, 3, 7, "a1 a3 b2" },
        { 2, 3, 8, "b2 a1 a3" },
        { 2, 5, 1, "a1 a3 a5 b2 b4" },
        { 2, 5, 2, "b2 b4 a1 a3 a5" },
        { 2, 5, 3, "a1 b2 a3 b4 a5" },
        { 2, 5, 4, "a1 b2 a3 b4 a5" },
        { 2, 5, 5, "a1 a3 a5 b2 b4" },
        { 2, 5, 6, "b2 b4 a1 a3 a5" },
        { 2, 5, 7, "a1 a3 a5 b2 b4" },
        { 2, 5, 8, "b2 b4 a1 a3 a5" },
        { 2, 8, 1, "a1 a7 a3 a5 b6 b4 b2 b8" },
        { 2, 8, 2, "b4 b6 b8 b2 a3 a7 a5 a1" },
        { 2, 8, 3, "a1 b2 a3 b4 a5 b6 a7 b8" },
        { 2, 8, 4, "a1 b2 a3 b4 a5 b6 a7 b8" },
        { 2, 8, 5, "a1 a7 a3 a5 b6 b4 b2 b8" },
        { 2, 8, 6, "b4 b6 b8 b2 a3 a7 a5 a1" },
        { 2, 8, 7, "a1 a7 a3 a5 b6 b4 b2 b8" },
        { 2, 8, 8, "b4 b6 b8 b2 a3 a7 a5 a1" },
        { 2, 17, 1, "a1 a11 a13 a15 a7 a9 a5 a3 a17 b10 b2 b14 b8 b6 b4 b16 b12" },
        { 2, 17, 2, "b6 b16 b8 b10 b12 b4 b2 b14 a15 a13 a1 a9 a7 a11 a3 a5 a17" },
        { 2, 17, 3, "a1 b2 a3 b4 a5 b6 a7 b8 a9 b10 a11 b12 a13 b14 a15 b16 a17" },
        { 2, 17, 4, "a1 b2 a3 b4 a5 b6 a7 b8 a9 b10 a11 b12 a13 b14 a15 b16 a17" },
        { 2, 17, 5, "a1 a11 a13 a15 a7 a9 a5 a3 a17 b10 b2 b14 b8 b6 b4 b16 b12" },
        { 2, 17, 6, "b6 b16 b8 b10 b12 b4 b2 b14 a15 a13 a1 a9 a7 a11 a3 a5 a17" },
        { 2, 17, 7, "a1 a11 a13 a15 a7 a9 a5 a3 a17 b10 b2 b14 b8 b6 b4 b16 b12" },
        { 2, 17, 8, "b6 b16 b8 b10 b12 b4 b2 b14 a15 a13 a1 a9 a7 a11 a3 a5 a17" },
        { 3, 2, 1, "11 22" },
        { 3, 2, 2, "22 11" },
        { 3, 2, 3, "11 22" },
        { 3, 2, 4, "22 11" },
        { 3, 2, 5, "11 22" },
        { 3, 2, 6, "22 11" },
        { 3, 2, 7, "11 22" },
        { 3, 2, 8, "22 11" },
        { 3, 3, 1, "03 11 22" },
        { 3, 3, 2, "22 11 03" },
        { 3, 3, 3, "03 11 22" },
        { 3, 3, 4, "22 11 03" },
        { 3, 3, 5, "03 11 22" },
        { 3, 3, 6, "22 11 03" },
        { 3, 3, 7, "03 11 22" },
        { 3, 3, 8, "22 11 03" },
        { 3, 5, 1, "03 14 11 22 25" },
        { 3, 5, 2, "25 22 11 14 03" },
        { 3, 5, 3, "03 14 11 22 25" },
        { 3, 5, 4, "25 22 11 14 03" },
        { 3, 5, 5, "03 14 11 22 25" },
        { 3, 5, 6, "25 22 11 14 03" },
        { 3, 5, 7, "03 14 11 22 25" },
        { 3, 5, 8, "25 22 11 14 03" },
        { 3, 8, 1, "06 03 11 14 17 25 22 28" },
        { 3, 8, 2, "28 22 25 14 11 17 06 03" },
        { 3, 8, 3, "06 03 11 14 17 25 22 28" },
        { 3, 8, 4, "28 22 25 14 11 17 06 03" },
        { 3, 8, 5, "06 03 11 14 17 25 22 28" },
        { 3, 8, 6, "28 22 25 14 11 17 06 03" },
        { 3, 8, 7, "06 03 11 14 17 25 22 28" },
        { 3, 8, 8, "28 22 25 14 11 17 06 03" },
        { 3, 17, 1, "09 06 03 015 012 113 110 116 11 17 14 25 22 214 28 211 217" },
        { 3, 17, 2, "217 211 25 28 214 22 110 113 11 17 116 14 06 012 015 03 09" },
        { 3, 17, 3, "09 06 03 015 012 113 110 116 11 17 14 25 22 214 28 211 217" },
        { 3, 17, 4, "217 211 25 28 214 22 110 113 11 17 116 14 06 012 015 03 09" },
        { 3, 17, 5, "09 06 03 015 012 113 110 116 11 17 14 25 22 214 28 211 217" },
        { 3, 17, 6, "217 211 25 28 214 22 110 113 11 17 116 14 06 012 015 03 09" },
        { 3, 17, 7, "09 06 03 015 012 113 110 116 11 17 14 25 22 214 28 211 217" },
        { 3, 17, 8, "217 211 25 28 214 22 110 113 11 17 116 14 06 012 015 03 09" },
        { 4, 2, 1, "a1 A2" },
        { 4, 2, 2, "a1 A2" },
        { 4, 2, 3, "a1 A2" },
        { 4, 2, 4, "a1 A2" },
        { 4, 2, 5, "a1 A2" },
        { 4, 2, 6, "a1 A2" },
        { 4, 2, 7, "a1 A2" },
        { 4, 2, 8, "A2 a1" },
        { 4, 3, 1, "a1 A2 a3" },
        { 4, 3, 2, "a1 A2 a3" },
        { 4, 3, 3, "a1 A2 a3" },
        { 4, 3, 4, "a1 A2 a3" },
        { 4, 3, 5, "a1 A2 a3" },
        { 4, 3, 6, "a1 A2 a3" },
        { 4, 3, 7, "a1 a3 A2" },
        { 4, 3, 8, "A2 a1 a3" },
        { 4, 5, 1, "a1 A2 a3 A4 a5" },
        { 4, 5, 2, "a1 A2 a3 A4 a5" },
        { 4, 5, 3, "a1 A2 a3 A4 a5" },
        { 4, 5, 4, "a1 A2 a3 A4 a5" },
        { 4, 5, 5, "a1 A2 a3 A4 a5" },
        { 4, 5, 6, "a1 A2 a3 A4 a5" },
        { 4, 5, 7, "a1 a3 a5 A2 A4" },
        { 4, 5, 8, "A2 A4 a1 a3 a5" },
        { 4, 8, 1, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 2, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 3, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 4, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 5, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 6, "a1 A2 a3 A4 a5 A6 a7 A8" },
        { 4, 8, 7, "a1 a7 a3 a5 A6 A4 A2 A8" },
        { 4, 8, 8, "A4 A6 A8 A2 a3 a7 a5 a1" },
        { 4, 17, 1, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 2, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 3, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 4, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 5, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 6, "a1 A2 a3 A4 a5 A6 a7 A8 a9 A10 a11 A12 a13 A14 a15 A16 a17" },
        { 4, 17, 7, "a1 a11 a13 a15 a7 a9 a5 a3 a17 A10 A2 A14 A8 A6 A4 A16 A12" },
        { 4, 17, 8, "A6 A16 A8 A10 A12 A4 A2 A14 a15 a13 a1 a9 a7 a11 a3 a5 a17" },
        { 5, 2, 1, "02 11" },
        { 5, 2, 2, "11 02" },
        { 5, 2, 3, "02 11" },
        { 5, 2, 4, "11 02" },
        { 5, 2, 5, "02 11" },
        { 5, 2, 6, "11 02" },
        { 5, 2, 7, "02 11" },
        { 5, 2, 8, "11 02" },
        { 5, 3, 1, "02 11 x3" },
        { 5, 3, 2, "x3 11 02" },
        { 5, 3, 3, "02 x3 11" },
        { 5, 3, 4, "11 02 x3" },
        { 5, 3, 5, "02 11 x3" },
        { 5, 3, 6, "x3 11 02" },
        { 5, 3, 7, "02 11 x3" },
        { 5, 3, 8, "x3 11 02" },
        { 5, 5, 1, "02 04 11 15 x3" },
        { 5, 5, 2, "x3 11 15 02 04" },
        { 5, 5, 3, "x3 02 04 11 15" },
        { 5, 5, 4, "11 15 04 02 x3" },
        { 5, 5, 5, "02 04 11 15 x3" },
        { 5, 5, 6, "x3 11 15 02 04" },
        { 5, 5, 7, "02 04 11 15 x3" },
        { 5, 5, 8, "x3 11 15 02 04" },
        { 5, 8, 1, "04 08 02 15 11 17 x6 x3" },
        { 5, 8, 2, "x3 x6 11 17 15 04 02 08" },
        { 5, 8, 3, "04 x6 08 02 x3 17 15 11" },
        { 5, 8, 4, "11 17 15 x6 04 02 x3 08" },
        { 5, 8, 5, "04 08 02 15 11 17 x6 x3" },
        { 5, 8, 6, "x3 x6 11 17 15 04 02 08" },
        { 5, 8, 7, "04 08 02 15 11 17 x6 x3" },
        { 5, 8, 8, "x3 x6 11 17 15 04 02 08" },
        { 5, 17, 1, "016 04 014 02 08 010 113 111 11 117 17 15 x15 x6 x3 x12 x9" },
        { 5, 17, 2, "x9 x6 x3 x15 x12 113 111 11 117 15 17 02 014 04 010 08 016" },
        { 5, 17, 3, "x9 08 016 010 014 x6 x15 02 x3 04 x12 113 11 17 15 111 117" },
        { 5, 17, 4, "11 113 111 17 117 15 02 014 x15 x12 x6 x3 016 010 04 08 x9" },
        { 5, 17, 5, "016 04 014 02 08 010 113 111 11 117 17 15 x15 x6 x3 x12 x9" },
        { 5, 17, 6, "x9 x6 x3 x15 x12 113 111 11 117 15 17 02 014 04 010 08 016" },
        { 5, 17, 7, "016 04 014 02 08 010 113 111 11 117 17 15 x15 x6 x3 x12 x9" },
        { 5, 17, 8, "x9 x6 x3 x15 x12 113 111 11 117 15 17 02 014 04 010 08 016" },
    };

    public static TheoryData<int, int, string, string> Custom => new()
    {
        { 2, 0, "1,1,2,2,-1", "k1 k2" },
        { 2, 1, "1,1,2,2,1;1,1,2,2,1;1,2,2,1,-1", "k2 k1" },
        { 2, 2, "1,1,2,2,0", "k1 k2" },
        { 2, 3, "1,1,2,2,-1", "k1 k2" },
        { 3, 0, "1,1,2,2,-1;2,2,3,3,1;1,1,2,2,0;1,1,3,3,-1;2,2,3,3,1;1,1,2,3,0", "k1 k3 k2" },
        { 3, 1, "1,1,2,2,1;1,1,2,2,1;1,2,3,3,1;2,1,3,2,1;1,3,2,2,-1", "k3 k2 k1" },
        { 3, 2, "1,1,2,2,0;2,2,3,3,0", "k1 k2 k3" },
        { 3, 3, "1,1,2,2,-1;2,2,3,3,-1", "k1 k2 k3" },
        { 5, 0, "1,1,2,2,-1;2,2,3,3,1;1,1,3,3,0;1,1,5,5,-1;3,3,5,5,1;2,2,4,5,0;3,4,4,5,-1;3,2,4,5,1;2,4,4,5,0;1,1,2,4,-1;1,1,2,4,1;4,2,5,3,0;4,2,5,3,-1", "k4 k1 k5 k2 k3" },
        { 5, 1, "1,1,2,2,1;1,1,3,3,1;1,3,5,5,1;3,1,5,3,1;2,2,4,3,1;3,4,4,3,-1;3,2,4,3,1;2,4,4,3,-1;1,5,2,4,-1;1,5,2,4,-1;4,2,5,1,-1;4,2,5,1,-1", "k5 k4 k3 k2 k1" },
        { 5, 2, "1,1,2,2,0;2,2,3,3,0;3,3,4,4,0;4,4,5,5,0", "k1 k2 k3 k4 k5" },
        { 5, 3, "1,1,2,2,-1;2,2,3,3,-1;3,3,4,4,-1;4,4,5,5,-1", "k1 k2 k3 k4 k5" },
        { 8, 0, "1,1,2,2,-1;2,2,3,3,1;1,1,4,4,0;1,1,8,8,-1;4,4,8,8,1;2,2,7,8,0;6,6,7,8,-1;3,3,7,8,1;5,5,7,8,0;4,7,7,8,-1;5,3,7,8,1;4,7,7,8,0;6,2,7,3,-1;6,2,8,4,1;7,3,8,2,0;6,4,7,3,-1;1,1,2,6,1;1,6,4,7,0;2,1,4,7,-1;2,5,3,1,1;2,5,3,1,0;3,5,4,7,-1;3,5,4,7,1", "k6 k1 k7 k5 k8 k4 k3 k2" },
        { 8, 1, "1,1,2,2,1;1,1,4,4,1;1,4,8,8,1;4,1,8,4,1;2,2,7,4,1;6,6,7,4,-1;3,3,7,4,1;5,5,7,4,-1;4,7,7,4,-1;5,3,7,4,1;4,7,7,4,-1;6,2,7,3,1;6,3,8,1,-1;7,2,8,1,-1;6,3,7,2,-1;1,8,2,6,-1;1,8,4,7,-1;2,6,4,7,1;2,5,3,7,1;2,5,3,7,1;1,8,3,7,-1;3,5,4,6,1;3,6,4,5,-1", "k8 k7 k6 k5 k4 k3 k2 k1" },
        { 8, 2, "1,1,2,2,0;2,2,3,3,0;3,3,4,4,0;4,4,5,5,0;5,5,6,6,0;6,6,7,7,0;7,7,8,8,0", "k1 k2 k3 k4 k5 k6 k7 k8" },
        { 8, 3, "1,1,2,2,-1;2,2,3,3,-1;3,3,4,4,-1;4,4,5,5,-1;5,5,6,6,-1;6,6,7,7,-1;7,7,8,8,-1", "k1 k2 k3 k4 k5 k6 k7 k8" },
    };

    private static Task Sort(FlexGridModel grid, int mode, Func<int, int, Task<int>>? compare = null)
    {
        var method = typeof(FlexGridModel).GetMethod("SortAsync");
        method.Should().NotBeNull("the measured Sort algorithm must be implemented");
        return (Task)method!.Invoke(grid, [mode, compare])!;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Backwards_ranges_keep_fixed_rows_and_sort_columns_left_to_right(bool backwards)
    {
        var grid = new FlexGridModel { Rows = 5, Cols = 3 };
        grid.SetText(0, 1, "fixed");
        grid.SetText(1, 1, "b");
        grid.SetText(2, 1, "a");
        grid.SetText(2, 2, "2");
        grid.SetText(3, 1, "a");
        grid.SetText(3, 2, "1");
        grid.SetText(4, 1, "outside");
        grid.Row = backwards ? 3 : 0;
        grid.RowSel = backwards ? 0 : 3;
        grid.Col = backwards ? 2 : 1;
        grid.ColSel = backwards ? 1 : 2;
        await Sort(grid, 1);
        Enumerable.Range(0, 5).Select(r => grid.GetText(r, 1)).Should().Equal("fixed", "a", "a", "b", "outside");
        grid.GetText(1, 2).Should().Be("1");
        grid.GetText(2, 2).Should().Be("2");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Unmeasured_row_metadata_movement_refuses_before_mutation(bool height)
    {
        var grid = new FlexGridModel { Rows = 3 };
        grid.SetText(1, 1, "b");
        grid.SetText(2, 1, "a");
        grid.RowSel = 2;
        if (height) grid.SetRowHeight(1, 500);
        else grid.SetCellFormat("cellfontbold", true);
        var act = () => Sort(grid, 1);
        await act.Should().ThrowAsync<NotImplementedException>();
        grid.GetText(1, 1).Should().Be("b");
        grid.GetText(2, 1).Should().Be("a");
    }

    [Theory]
    [MemberData(nameof(Orders))]
    public async Task Recorded_builtin_order(int pattern, int count, int mode, string expected)
    {
        var grid = new FlexGridModel { Rows = count + 1, Cols = 3 };
        for (var r = 1; r <= count; r++)
        {
            var key = pattern switch
            {
                1 => "k",
                2 => r % 2 == 1 ? "a" : "b",
                3 => (r % 3).ToString(),
                4 => r % 2 == 1 ? "a" : "A",
                _ => r % 3 == 0 ? "x" : r % 2 == 1 ? "1" : "0",
            };
            grid.SetText(r, 1, key);
            grid.SetText(r, 0, key + r);
            grid.SetText(r, 2, r.ToString());
        }
        grid.SetText(0, 0, "fixed");
        grid.RowSel = count;
        await Sort(grid, mode);
        Enumerable.Range(1, count).Select(r => grid.GetText(r, 0)).Should().Equal(expected.Split(' '));
        foreach (var r in Enumerable.Range(1, count))
            grid.GetText(r, 0).Should().Be(grid.GetText(r, 1) + grid.GetText(r, 2));
        grid.GetText(0, 0).Should().Be("fixed");
    }

    [Theory]
    [MemberData(nameof(Custom))]
    public async Task Recorded_custom_order_and_every_Compare(int count, int handler, string expectedLog, string expectedOrder)
    {
        var grid = new FlexGridModel { Rows = count + 1 };
        for (var r = 1; r <= count; r++) grid.SetText(r, 1, r.ToString());
        grid.RowSel = count;
        var log = new List<string>();
        await Sort(grid, 9, (a, b) =>
        {
            var first = int.Parse(grid.GetText(a, 1));
            var second = int.Parse(grid.GetText(b, 1));
            var cmp = handler switch
            {
                0 => new[] { -1, 1, 0 }[log.Count % 3],
                1 => second.CompareTo(first),
                2 => 0,
                _ => first.CompareTo(second),
            };
            log.Add($"{a},{first},{b},{second},{cmp}");
            return Task.FromResult(cmp);
        });
        log.Should().Equal(expectedLog.Split(';'));
        Enumerable.Range(1, count).Select(r => "k" + grid.GetText(r, 1)).Should().Equal(expectedOrder.Split(' '));
    }
}
