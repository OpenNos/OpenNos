using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.DAL.EF;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    //TODO REVIEW MY RUSHED CODE
    public static class ShellGeneratorHelper
    {
        // VNUM -> ShellType
        public static readonly Dictionary<short, byte> ShellTypes = new Dictionary<short, byte>
        {
            {565, 0}, {566, 0}, {567, 0}, /* ARMOR */ {577, 1}, {578, 1}, {579, 1}, // SUPER SHELL
            {568, 2}, {569, 2}, {570, 2}, /* ARMOR */ {580, 3}, {581, 3}, {582, 3}, // SPECIAL SHELL
            {571, 4}, {572, 4}, {573, 4}, /* ARMOR */ {583, 5}, {584, 5}, {585, 5}, // PVP SHELL
            {574, 6}, {575, 6}, {576, 6}, /* ARMOR */ {586, 7}, {587, 7}, {588, 7}, // PERFECT SHELL
            {589, 8}, {590, 8}, {591, 8}, {592, 8}, {593, 8}, {594, 8}, {595, 8}, {596, 8}, {597, 8}, {598, 8}, /* HALF SHELL */
            {599, 9}, {600, 9}, {601, 9}, {602, 9}, {603, 9}, {604, 9}, {605, 9}, {606, 9}, {607, 9}, {608, 9}, /* HALF SHELL */
        };

        public static readonly Dictionary<int, List<object>> ShellType = new Dictionary<int, List<object>>
        {
            { 0, new List<object>{ 0, 0, 0, 0, 0 }},
            { 1, new List<object>{ 1, 50, 70, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 2, new List<object>{ 2, 50, 70, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 3, new List<object>{ 3, 50, 70, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 4, new List<object>{ 4, 50, 70, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 5, new List<object>{ 5, 50, 70, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 6, new List<object>{ 6, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 7, new List<object>{ 7, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 8, new List<object>{ 1, 50, 70, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 9, new List<object>{ 2, 50, 70, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 10, new List<object>{ 3, 50, 70, 13, 1, 14, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 11, new List<object>{ 4, 50, 70, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 12, new List<object>{ 5, 50, 70, 13, 1, 14, 1, 15, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 13, new List<object>{ 6, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 14, new List<object>{ 7, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 15, new List<object>{ 1, 71, 80, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 16, new List<object>{ 2, 71, 80, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 17, new List<object>{ 3, 71, 80, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 18, new List<object>{ 4, 71, 80, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 19, new List<object>{ 5, 71, 80, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 20, new List<object>{ 6, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 21, new List<object>{ 7, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 22, new List<object>{ 1, 71, 80, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 23, new List<object>{ 2, 71, 80, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 24, new List<object>{ 3, 71, 80, 13, 1, 14, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 25, new List<object>{ 4, 71, 80, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 26, new List<object>{ 5, 71, 80, 13, 1, 14, 1, 15, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 27, new List<object>{ 6, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 28, new List<object>{ 7, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 29, new List<object>{ 1, 81, 90, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 30, new List<object>{ 2, 81, 90, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 31, new List<object>{ 3, 81, 90, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 32, new List<object>{ 4, 81, 90, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 33, new List<object>{ 5, 81, 90, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 34, new List<object>{ 6, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 35, new List<object>{ 7, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 36, new List<object>{ 1, 81, 90, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 37, new List<object>{ 2, 81, 90, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 38, new List<object>{ 3, 81, 90, 13, 1, 14, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 39, new List<object>{ 4, 81, 90, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 40, new List<object>{ 5, 81, 90, 13, 1, 14, 1, 15, 1, 12, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 41, new List<object>{ 6, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 42, new List<object>{ 7, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 43, new List<object>{ 1, 50, 70, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 44, new List<object>{ 2, 50, 70, 1, 1, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 45, new List<object>{ 3, 50, 70, 1, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 46, new List<object>{ 4, 50, 70, 1, 1, 2, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 47, new List<object>{ 5, 50, 70, 1, 1, 2, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 48, new List<object>{ 6, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 49, new List<object>{ 7, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 1, null, null, null, null, null, null, null, null, null, null }},
            { 50, new List<object>{ 1, 50, 70, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 51, new List<object>{ 2, 50, 70, 13, 1, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 52, new List<object>{ 3, 50, 70, 13, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 53, new List<object>{ 4, 50, 70, 13, 1, 14, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 54, new List<object>{ 5, 50, 70, 13, 1, 14, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 55, new List<object>{ 6, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 56, new List<object>{ 7, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 1, null, null, null, null, null, null, null, null, null, null }},
            { 57, new List<object>{ 1, 71, 80, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 58, new List<object>{ 2, 71, 80, 1, 1, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 59, new List<object>{ 3, 71, 80, 1, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 60, new List<object>{ 4, 71, 80, 1, 1, 2, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 61, new List<object>{ 5, 71, 80, 1, 1, 2, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 62, new List<object>{ 6, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 63, new List<object>{ 7, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 1, null, null, null, null, null, null, null, null, null, null }},
            { 64, new List<object>{ 1, 71, 80, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 65, new List<object>{ 2, 71, 80, 13, 1, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 66, new List<object>{ 3, 71, 80, 13, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 67, new List<object>{ 4, 71, 80, 13, 1, 14, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 68, new List<object>{ 5, 71, 80, 13, 1, 14, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 69, new List<object>{ 6, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 70, new List<object>{ 7, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 1, null, null, null, null, null, null, null, null, null, null }},
            { 71, new List<object>{ 1, 81, 90, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 72, new List<object>{ 2, 81, 90, 1, 1, 1, 1, 5, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 73, new List<object>{ 3, 81, 90, 1, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 74, new List<object>{ 4, 81, 90, 1, 1, 2, 1, 2, 1, 6, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 75, new List<object>{ 5, 81, 90, 1, 1, 2, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 76, new List<object>{ 6, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, 7, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 77, new List<object>{ 7, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 1, null, null, null, null, null, null, null, null, null, null }},
            { 78, new List<object>{ 1, 81, 90, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 79, new List<object>{ 2, 81, 90, 13, 1, 13, 1, 17, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 80, new List<object>{ 3, 81, 90, 13, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 81, new List<object>{ 4, 81, 90, 13, 1, 14, 1, 14, 1, 18, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 82, new List<object>{ 5, 81, 90, 13, 1, 14, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 83, new List<object>{ 6, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, 19, 1, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 84, new List<object>{ 7, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 1, null, null, null, null, null, null, null, null, null, null }},
            { 85, new List<object>{ 1, 50, 70, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 86, new List<object>{ 2, 50, 70, 1, 1, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 87, new List<object>{ 3, 50, 70, 1, 1, 2, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 88, new List<object>{ 4, 50, 70, 1, 1, 2, 1, 2, 1, 10, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 89, new List<object>{ 5, 50, 70, 1, 1, 2, 1, 3, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 90, new List<object>{ 6, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, 11, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null }},
            { 91, new List<object>{ 7, 50, 70, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 12, 1, 12, 1, 11, 1, 11, 1, 10, 0, null, null }},
            { 92, new List<object>{ 1, 50, 70, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 93, new List<object>{ 2, 50, 70, 13, 1, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 94, new List<object>{ 3, 50, 70, 13, 1, 14, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 95, new List<object>{ 4, 50, 70, 13, 1, 14, 1, 14, 1, 22, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 96, new List<object>{ 5, 50, 70, 13, 1, 14, 1, 15, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 97, new List<object>{ 6, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, 23, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null }},
            { 98, new List<object>{ 7, 50, 70, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 24, 1, 24, 1, 23, 1, 23, 1, 22, 0, null, null }},
            { 99, new List<object>{ 1, 71, 80, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 100, new List<object>{ 2, 71, 80, 1, 1, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 101, new List<object>{ 3, 71, 80, 1, 1, 2, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 102, new List<object>{ 4, 71, 80, 1, 1, 2, 1, 2, 1, 10, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 103, new List<object>{ 5, 71, 80, 1, 1, 2, 1, 3, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 104, new List<object>{ 6, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, 11, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null }},
            { 105, new List<object>{ 7, 71, 80, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 12, 1, 12, 1, 11, 1, 11, 1, 10, 0, null, null }},
            { 106, new List<object>{ 1, 71, 80, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 107, new List<object>{ 2, 71, 80, 13, 1, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 108, new List<object>{ 3, 71, 80, 13, 1, 14, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 109, new List<object>{ 4, 71, 80, 13, 1, 14, 1, 14, 1, 22, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 110, new List<object>{ 5, 71, 80, 13, 1, 14, 1, 15, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 111, new List<object>{ 6, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, 23, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null }},
            { 112, new List<object>{ 7, 71, 80, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 24, 1, 24, 1, 23, 1, 23, 1, 22, 0, null, null }},
            { 113, new List<object>{ 1, 81, 90, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 114, new List<object>{ 2, 81, 90, 1, 1, 1, 1, 9, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 115, new List<object>{ 3, 81, 90, 1, 1, 2, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 116, new List<object>{ 4, 81, 90, 1, 1, 2, 1, 2, 1, 10, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 117, new List<object>{ 5, 81, 90, 1, 1, 2, 1, 3, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 118, new List<object>{ 6, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, 11, 1, 11, 1, 10, 0, null, null, null, null, null, null, null, null }},
            { 119, new List<object>{ 7, 81, 90, 1, 1, 2, 1, 3, 1, 3, 1, 4, 1, 12, 1, 12, 1, 11, 1, 11, 1, 10, 0, null, null }},
            { 120, new List<object>{ 1, 81, 90, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 121, new List<object>{ 2, 81, 90, 13, 1, 13, 1, 21, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 122, new List<object>{ 3, 81, 90, 13, 1, 14, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 123, new List<object>{ 4, 81, 90, 13, 1, 14, 1, 14, 1, 22, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 124, new List<object>{ 5, 81, 90, 13, 1, 14, 1, 15, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 125, new List<object>{ 6, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, 23, 1, 23, 1, 22, 0, null, null, null, null, null, null, null, null }},
            { 126, new List<object>{ 7, 81, 90, 13, 1, 14, 1, 15, 1, 15, 1, 16, 1, 24, 1, 24, 1, 23, 1, 23, 1, 22, 0, null, null }},
            { 127, new List<object>{ 1, 50, 70, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 128, new List<object>{ 2, 50, 70, 1, 1, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 129, new List<object>{ 3, 50, 70, 1, 1, 1, 1, 2, 1, 6, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 130, new List<object>{ 4, 50, 70, 1, 1, 1, 1, 2, 1, 2, 1, 6, 0, 10, 0, 10, 0, null, null, null, null, null, null, null, null }},
            { 131, new List<object>{ 5, 50, 70, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 7, 0, 11, 0, 10, 0, null, null, null, null, null, null }},
            { 132, new List<object>{ 6, 50, 70, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 7, 0, 11, 0, 11, 0, 10, 0, null, null }},
            { 133, new List<object>{ 7, 50, 70, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 0, 12, 0, 12, 0, 11, 0 }},
            { 134, new List<object>{ 1, 50, 70, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 135, new List<object>{ 2, 50, 70, 13, 1, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 136, new List<object>{ 3, 50, 70, 13, 1, 13, 1, 14, 1, 18, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 137, new List<object>{ 4, 50, 70, 13, 1, 13, 1, 14, 1, 14, 1, 18, 0, 22, 0, 22, 0, null, null, null, null, null, null, null, null }},
            { 138, new List<object>{ 5, 50, 70, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 19, 0, 23, 0, 22, 0, 12, 0, null, null, null, null }},
            { 139, new List<object>{ 6, 50, 70, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 19, 0, 23, 0, 23, 0, 22, 0, null, null }},
            { 140, new List<object>{ 7, 50, 70, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 0, 24, 0, 24, 0, 23, 0 }},
            { 141, new List<object>{ 1, 71, 80, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 142, new List<object>{ 2, 71, 80, 1, 1, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 143, new List<object>{ 3, 71, 80, 1, 1, 1, 1, 2, 1, 6, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 144, new List<object>{ 4, 71, 80, 1, 1, 1, 1, 2, 1, 2, 1, 6, 0, 10, 0, 10, 0, null, null, null, null, null, null, null, null }},
            { 145, new List<object>{ 5, 71, 80, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 7, 0, 11, 0, 10, 0, null, null, null, null, null, null }},
            { 146, new List<object>{ 6, 71, 80, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 7, 0, 11, 0, 11, 0, 10, 0, null, null }},
            { 147, new List<object>{ 7, 71, 80, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 0, 12, 0, 12, 0, 11, 0 }},
            { 148, new List<object>{ 1, 71, 80, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 149, new List<object>{ 2, 71, 80, 13, 1, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 150, new List<object>{ 3, 71, 80, 13, 1, 13, 1, 14, 1, 18, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 151, new List<object>{ 4, 71, 80, 13, 1, 13, 1, 14, 1, 14, 1, 18, 0, 22, 0, 22, 0, null, null, null, null, null, null, null, null }},
            { 152, new List<object>{ 5, 71, 80, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 19, 0, 23, 0, 22, 0, 12, 0, null, null, null, null }},
            { 153, new List<object>{ 6, 71, 80, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 19, 0, 23, 0, 23, 0, 22, 0, null, null }},
            { 154, new List<object>{ 7, 71, 80, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 0, 24, 0, 24, 0, 23, 0 }},
            { 155, new List<object>{ 1, 81, 90, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 156, new List<object>{ 2, 81, 90, 1, 1, 1, 1, 5, 0, 9, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 157, new List<object>{ 3, 81, 90, 1, 1, 1, 1, 2, 1, 6, 0, 10, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 158, new List<object>{ 4, 81, 90, 1, 1, 1, 1, 2, 1, 2, 1, 6, 0, 10, 0, 10, 0, null, null, null, null, null, null, null, null }},
            { 159, new List<object>{ 5, 81, 90, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 7, 0, 11, 0, 10, 0, null, null, null, null, null, null }},
            { 160, new List<object>{ 6, 81, 90, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 7, 0, 11, 0, 11, 0, 10, 0, null, null }},
            { 161, new List<object>{ 7, 81, 90, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, 4, 1, 7, 0, 12, 0, 12, 0, 11, 0 }},
            { 162, new List<object>{ 1, 81, 90, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 163, new List<object>{ 2, 81, 90, 13, 1, 13, 1, 17, 0, 21, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 164, new List<object>{ 3, 81, 90, 13, 1, 13, 1, 14, 1, 18, 0, 22, 0, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 165, new List<object>{ 4, 81, 90, 13, 1, 13, 1, 14, 1, 14, 1, 18, 0, 22, 0, 22, 0, null, null, null, null, null, null, null, null }},
            { 166, new List<object>{ 5, 81, 90, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 19, 0, 23, 0, 22, 0, 12, 0, null, null, null, null }},
            { 167, new List<object>{ 6, 81, 90, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 19, 0, 23, 0, 23, 0, 22, 0, null, null }},
            { 168, new List<object>{ 7, 81, 90, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, 16, 1, 19, 0, 24, 0, 24, 0, 23, 0 }},
            { 169, new List<object>{ 1, 25, 25, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 170, new List<object>{ 2, 25, 25, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 171, new List<object>{ 3, 25, 25, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 172, new List<object>{ 4, 25, 25, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 173, new List<object>{ 5, 25, 25, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 174, new List<object>{ 6, 25, 25, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 175, new List<object>{ 7, 25, 25, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 176, new List<object>{ 1, 25, 25, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 177, new List<object>{ 2, 25, 25, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 178, new List<object>{ 3, 25, 25, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 179, new List<object>{ 4, 25, 25, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 180, new List<object>{ 5, 25, 25, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 181, new List<object>{ 6, 25, 25, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 182, new List<object>{ 7, 25, 25, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 183, new List<object>{ 1, 30, 30, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 184, new List<object>{ 2, 30, 30, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 185, new List<object>{ 3, 30, 30, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 186, new List<object>{ 4, 30, 30, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 187, new List<object>{ 5, 30, 30, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 188, new List<object>{ 6, 30, 30, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 189, new List<object>{ 7, 30, 30, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 190, new List<object>{ 1, 30, 30, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 191, new List<object>{ 2, 30, 30, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 192, new List<object>{ 3, 30, 30, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 193, new List<object>{ 4, 30, 30, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 194, new List<object>{ 5, 30, 30, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 195, new List<object>{ 6, 30, 30, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 196, new List<object>{ 7, 30, 30, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 197, new List<object>{ 1, 45, 45, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 198, new List<object>{ 2, 45, 45, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 199, new List<object>{ 3, 45, 45, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 200, new List<object>{ 4, 45, 45, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 201, new List<object>{ 5, 45, 45, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 202, new List<object>{ 6, 45, 45, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 203, new List<object>{ 7, 45, 45, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 204, new List<object>{ 1, 45, 45, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 205, new List<object>{ 2, 45, 45, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 206, new List<object>{ 3, 45, 45, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 207, new List<object>{ 4, 45, 45, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 208, new List<object>{ 5, 45, 45, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 209, new List<object>{ 6, 45, 45, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 210, new List<object>{ 7, 45, 45, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 211, new List<object>{ 1, 55, 55, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 212, new List<object>{ 2, 55, 55, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 213, new List<object>{ 3, 55, 55, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 214, new List<object>{ 4, 55, 55, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 215, new List<object>{ 5, 55, 55, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 216, new List<object>{ 6, 55, 55, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 217, new List<object>{ 7, 55, 55, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 218, new List<object>{ 1, 55, 55, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 219, new List<object>{ 2, 55, 55, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 220, new List<object>{ 3, 55, 55, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 221, new List<object>{ 4, 55, 55, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 222, new List<object>{ 5, 55, 55, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 223, new List<object>{ 6, 55, 55, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 224, new List<object>{ 7, 55, 55, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 225, new List<object>{ 1, 60, 60, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 226, new List<object>{ 2, 60, 60, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 227, new List<object>{ 3, 60, 60, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 228, new List<object>{ 4, 60, 60, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 229, new List<object>{ 5, 60, 60, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 230, new List<object>{ 6, 60, 60, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 231, new List<object>{ 7, 60, 60, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 232, new List<object>{ 1, 60, 60, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 233, new List<object>{ 2, 60, 60, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 234, new List<object>{ 3, 60, 60, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 235, new List<object>{ 4, 60, 60, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 236, new List<object>{ 5, 60, 60, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 237, new List<object>{ 6, 60, 60, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 238, new List<object>{ 7, 60, 60, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 239, new List<object>{ 1, 65, 65, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 240, new List<object>{ 2, 65, 65, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 241, new List<object>{ 3, 65, 65, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 242, new List<object>{ 4, 65, 65, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 243, new List<object>{ 5, 65, 65, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 244, new List<object>{ 6, 65, 65, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 245, new List<object>{ 7, 65, 65, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 246, new List<object>{ 1, 65, 65, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 247, new List<object>{ 2, 65, 65, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 248, new List<object>{ 3, 65, 65, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 249, new List<object>{ 4, 65, 65, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 250, new List<object>{ 5, 65, 65, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 251, new List<object>{ 6, 65, 65, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 252, new List<object>{ 7, 65, 65, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 253, new List<object>{ 1, 70, 70, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 254, new List<object>{ 2, 70, 70, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 255, new List<object>{ 3, 70, 70, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 256, new List<object>{ 4, 70, 70, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 257, new List<object>{ 5, 70, 70, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 258, new List<object>{ 6, 70, 70, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 259, new List<object>{ 7, 70, 70, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 260, new List<object>{ 1, 70, 70, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 261, new List<object>{ 2, 70, 70, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 262, new List<object>{ 3, 70, 70, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 263, new List<object>{ 4, 70, 70, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 264, new List<object>{ 5, 70, 70, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 265, new List<object>{ 6, 70, 70, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 266, new List<object>{ 7, 70, 70, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 267, new List<object>{ 1, 75, 75, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 268, new List<object>{ 2, 75, 75, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 269, new List<object>{ 3, 75, 75, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 270, new List<object>{ 4, 75, 75, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 271, new List<object>{ 5, 75, 75, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 272, new List<object>{ 6, 75, 75, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 273, new List<object>{ 7, 75, 75, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 274, new List<object>{ 1, 75, 75, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 275, new List<object>{ 2, 75, 75, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 276, new List<object>{ 3, 75, 75, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 277, new List<object>{ 4, 75, 75, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 278, new List<object>{ 5, 75, 75, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 279, new List<object>{ 6, 75, 75, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 280, new List<object>{ 7, 75, 75, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 281, new List<object>{ 1, 80, 80, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 282, new List<object>{ 2, 80, 80, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 283, new List<object>{ 3, 80, 80, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 284, new List<object>{ 4, 80, 80, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 285, new List<object>{ 5, 80, 80, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 286, new List<object>{ 6, 80, 80, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 287, new List<object>{ 7, 80, 80, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 288, new List<object>{ 1, 80, 80, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 289, new List<object>{ 2, 80, 80, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 290, new List<object>{ 3, 80, 80, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 291, new List<object>{ 4, 80, 80, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 292, new List<object>{ 5, 80, 80, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 293, new List<object>{ 6, 80, 80, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 294, new List<object>{ 7, 80, 80, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }},
            { 295, new List<object>{ 1, 85, 85, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 296, new List<object>{ 2, 85, 85, 1, 1, 1, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 297, new List<object>{ 3, 85, 85, 1, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 298, new List<object>{ 4, 85, 85, 1, 1, 2, 1, 2, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 299, new List<object>{ 5, 85, 85, 1, 1, 2, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 300, new List<object>{ 6, 85, 85, 1, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 301, new List<object>{ 7, 85, 85, 1, 1, 1, 1, 2, 1, 2, 1, 3, 1, 3, 1, null, null, null, null, null, null, null, null, null, null }},
            { 302, new List<object>{ 1, 85, 85, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 303, new List<object>{ 2, 85, 85, 13, 1, 13, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 304, new List<object>{ 3, 85, 85, 13, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 305, new List<object>{ 4, 85, 85, 13, 1, 14, 1, 14, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 306, new List<object>{ 5, 85, 85, 13, 1, 14, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 307, new List<object>{ 6, 85, 85, 13, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 308, new List<object>{ 7, 85, 85, 13, 1, 13, 1, 14, 1, 14, 1, 15, 1, 15, 1, null, null, null, null, null, null, null, null, null, null }}
        };

        public static readonly Dictionary<int, List<object>> ShellOptionLevel = new Dictionary<int, List<object>>() {
            { 0, new List<object>{ "0", 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
            { 1, new List<object>{ "C", 1, 3, 6, 9, 10, 11, 15, 16, 17, 23, null, null, null, null, null, null, null, null, null, null }},
            { 2, new List<object>{ "B", 1, 4, 7, 9, 10, 11, 12, 13, 18, 19, 20, 21, 23, 24, 25, 26, 27, 28, 29, null }},
            { 3, new List<object>{ "A", 1, 5, 8, 12, 13, 18, 19, 20, 21, 24, 25, 26, 27, 28, 29, null, null, null, null, null }},
            { 4, new List<object>{ "S", 2, 14, 22, 30, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 5, new List<object>{ "C", 31, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 6, new List<object>{ "B", 31, 32, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 7, new List<object>{ "A", 31, 32, 33, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 8, new List<object>{ "S", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 9, new List<object>{ "C", 34, 35, 42, 43, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 10, new List<object>{ "B", 34, 35, 36, 37, 38, 39, 43, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 11, new List<object>{ "A", 34, 35, 36, 37, 38, 39, 43, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 12, new List<object>{ "S", 34, 35, 40, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 13, new List<object>{ "C", 51, 52, 53, 55, 58, 62, 65, 66, 68, 70, 73, null, null, null, null, null, null, null, null, null }},
            { 14, new List<object>{ "B", 51, 52, 53, 56, 59, 60, 61, 62, 63, 64, 68, 70, 73, 74, 75, 76, 77, null, null, null }},
            { 15, new List<object>{ "A", 51, 52, 53, 57, 59, 60, 61, 69, 71, 74, 75, 76, 77, null, null, null, null, null, null, null }},
            { 16, new List<object>{ "S", 54, 67, 72, 78, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 17, new List<object>{ "C", 79, 80, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 18, new List<object>{ "B", 80, 81, 82, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 19, new List<object>{ "A", 81, 82, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 20, new List<object>{ "S", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 21, new List<object>{ "C", 83, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 22, new List<object>{ "B", 84, 85, 86, 83, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 23, new List<object>{ "A", 83, 84, 85, 86, 88, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }},
            { 24, new List<object>{ "S", 83, 87, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }}
        };

        public static readonly Dictionary<int, List<object>> ShellOptionType = new Dictionary<int, List<object>>
        {
            { 0, new List<object>{ 0, 0, 0, 0, 0 }},
            { 1, new List<object>{ "Dégâts supplémentaires", "X", 835, 1156, 1187, 1502, 1468, 2101, null, null }},
            { 2, new List<object>{ "% pour les dégâts", "X%", null, null, null, null, null, null, 88, 212 }},
            { 3, new List<object>{ "Diminue la chance d\"exposition aux petites plaies", "X%", 18, 52, null, null, null, null, null, null }},
            { 4, new List<object>{ "Diminue la chance d\"être exposé aux plaies en général", "X%", null, null, 18, 52, null, null, null, null }},
            { 5, new List<object>{ "Diminue la chance d\"être exposé aux plaies béantes en général", "X%", null, null, null, null, 2, 18, null, null }},
            { 6, new List<object>{ "Ogłuszenie", "X%", 18, 52, null, null, null, null, null, null }},
            { 7, new List<object>{ "Zamrożenie", "X%", null, null, 18, 52, null, null, null, null }},
            { 8, new List<object>{ "Silne ogłuszenie", "X%", null, null, null, null, 18, 52, null, null }},
            { 9, new List<object>{ "Augmente les dégâts sur les plantes", "X%", 60, 104, 110, 204, null, null, null, null }},
            { 10, new List<object>{ "Augmente les dégâts sur les animaux", "X%", 60, 104, 110, 204, null, null, null, null }},
            { 11, new List<object>{ "Augmente les dégâts sur les démons", "X%", 60, 104, 110, 204, null, null, null, null }},
            { 12, new List<object>{ "Augmente les dégâts sur les zombies", "X%", null, null, 60, 104, 110, 204, null, null }},
            { 13, new List<object>{ "Zwiększa szkody przy małych zwierzętach", "X%", null, null, 60, 104, 110, 204, null, null }},
            { 14, new List<object>{ "Augmente les dégâts sur les monstres géants", "X%", null, null, null, null, null, null, 229, 256 }},
            { 15, new List<object>{ "(poza różdżkami) Zwiększona szansa krytycznego uderzenia", "X%", 30, 104, null, null, null, null, null, null }},
            { 16, new List<object>{ "(poza różdżkami) Zwiększone krytyczne obrażenia", "X%", 342, 645, null, null, null, null, null, null }},
            { 17, new List<object>{ "(tylko różdżki) Niezakłócony podczas rzucania czarów", "X", null, null, null, null, null, null, null, null }},
            { 18, new List<object>{ "Augmente l\"élément feu", "X", null, null, 482, 802, 915, 1545, null, null }},
            { 19, new List<object>{ "Augmente l\"élément eau", "X", null, null, 482, 802, 915, 1545, null, null }},
            { 20, new List<object>{ "Augmente l\"élément lumière", "X", null, null, 482, 802, 915, 1545, null, null }},
            { 21, new List<object>{ "Augmente l\"élément d\"obscurité", "X", null, null, 482, 802, 915, 1545, null, null }},
            { 22, new List<object>{ "Zwiększona energia wszystkich elementów", "X", null, null, null, null, null, null, 1187, 1778 }},
            { 23, new List<object>{ "Zredukowane zużycie PM", "X%", 70, 118, 162, 232, null, null, null, null }},
            { 24, new List<object>{ "Odnowienie PŻ za zabicie", "X", null, null, 1468, 1504, 1961, 2004, null, null }},
            { 25, new List<object>{ "Regeneracja PM za zabicie", "X", null, null, 1468, 1504, 1961, 2004, null, null }},
            { 26, new List<object>{ "Zwiększa SL obrażeń", "X", null, null, 88, 123, 130, 177, null, null }},
            { 27, new List<object>{ "Zwiększa SL obrony", "X", null, null, 88, 123, 130, 177, null, null }},
            { 28, new List<object>{ "Zwiększa SL siły", "X", null, null, 88, 123, 130, 177, null, null }},
            { 29, new List<object>{ "Zwiększa SL energii", "X", null, null, 88, 123, 130, 177, null, null }},
            { 30, new List<object>{ "Zwiększa ogólny SL", "X", null, null, null, null, null, null, 88, 118 }},
            { 31, new List<object>{ "(tylko broń główna) Zdobycie większej ilości złota", "X%", 46, 102, 130, 177, 224, 347, null, null }},
            { 32, new List<object>{ "(tylko broń główna) Zdobycie większej ilości punktów DOŚW", "X%", null, null, 60, 104, 88, 156, null, null }},
            { 33, new List<object>{ "(tylko broń główna)Zdobycie większej ilości PP", "X%", null, null, 60, 104, 88, 156, null, null }},
            { 34, new List<object>{ "% do Obrażeń w PVP", "X%", 88, 118, 39, 104, 88, 177, 187, 340 }},
            { 35, new List<object>{ "Obniża obronę o % w PVP", "X%", 88, 115, 88, 152, 113, 204, 212, 347 }},
            { 36, new List<object>{ "Obniża odporność przeciwnika na ogień w PVP", "X%", null, null, 18, 77, 60, 147, null, null }},
            { 37, new List<object>{ "Obniża odporność przeciwnika na wodę w PVP", "X%", null, null, 18, 77, 60, 147, null, null }},
            { 38, new List<object>{ "Obniża odporność przeciwnika na światło w PVP", "X%", null, null, 18, 77, 60, 147, null, null }},
            { 39, new List<object>{ "Obniża odporność przeciwnika na mrok w PVP", "X%", null, null, 18, 77, 60, 147, null, null }},
            { 40, new List<object>{ "Obniża całą odporność przeciwnika w PVP", "X%", null, null, null, null, null, null, 88, 178 }},
            { 41, new List<object>{ "Trafia za każdym razem w PVP", "X", null, null, null, null, null, null, null, null }},
            { 42, new List<object>{ "% de dégâts à 15% en PvP", "X%", 483, 602, null, null, null, null, null, null }},
            { 43, new List<object>{ "Odbierz przeciwnikowi manę w PVP", "X", 88, 190, 229, 304, 360, 502, null, null }},
            { 44, new List<object>{ "Ignoruje odporność na ogień w PVP z prawdopodobieństwem 25%", "X%", null, null, null, null, null, null, null, null }},
            { 45, new List<object>{ "Ignoruje odporność na wodę w PVP z prawdopodobieństwem 25%", "X%", null, null, null, null, null, null, null, null }},
            { 46, new List<object>{ "Ignoruje odporność na światło w PVP z prawdopodobieństwem 25%", "X%", null, null, null, null, null, null, null, null }},
            { 47, new List<object>{ "Ignoruje odporność na mrok w PVP z prawdopodobieństwem 25%", "X%", null, null, null, null, null, null, null, null }},
            { 48, new List<object>{ "Odnowienie PS za zabicie", "X", null, null, null, null, null, null, null, null }},
            { 49, new List<object>{ "Zwiększ celność", "X", null, null, null, null, null, null, null, null }},
            { 50, new List<object>{ "Zwiększa koncentrację", "X", null, null, null, null, null, null, null, null }},
            { 51, new List<object>{ "Zwiększona obrona wręcz", "X", 384, 690, 680, 1204, 1471, 2002, null, null }},
            { 52, new List<object>{ "Zwiększona obrona długodystansowa", "X", 384, 690, 680, 1204, 1471, 2002, null, null }},
            { 53, new List<object>{ "Zwiększona obrona magiczna", "X", 384, 690, 680, 1204, 1471, 2002, null, null }},
            { 54, new List<object>{ "% do ogólnej obrony", "X%", null, null, null, null, null, null, 187, 256 }},
            { 55, new List<object>{ "Zmniejsza szansę małej otwartej rany", "X%", 187, 477, null, null, null, null, null, null }},
            { 56, new List<object>{ "Zmniejsza szansę otwartej rany i małej otwartej rany", "X%", null, null, 187, 477, null, null, null, null }},
            { 57, new List<object>{ "Zmniejsza szansę wszystkich otwartych ran", "X%", null, null, null, null, 187, 477, null, null }},
            { 58, new List<object>{ "Zmniejsza szansę ogłuszenia", "X%", 187, 477, null, null, null, null, null, null }},
            { 59, new List<object>{ "Zmniejsza szansę wszystkich zamroczeń", "X%", null, null, 187, 404, 285, 504, null, null }},
            { 60, new List<object>{ "Zmniejsza szansę ręki śmierci", "X%", null, null, 285, 502, 384, 602, null, null }},
            { 61, new List<object>{ "Zmniejsza szansę bycia zamrożonym", "X%", null, null, 187, 304, 360, 602, null, null }},
            { 62, new List<object>{ "Zmniejsza szansę bycia oślepionym", "X%", 285, 502, 384, 602, null, null, null, null }},
            { 63, new List<object>{ "Zmniejsza szansę skurczu", "X%", null, null, 384, 602, null, null, null, null }},
            { 64, new List<object>{ "Zmniejsza szansę słabego pancerza", "X%", null, null, 384, 602, null, null, null, null }},
            { 65, new List<object>{ "Zmniejsza szansę szoku", "X%", 384, 602, null, null, null, null, null, null }},
            { 66, new List<object>{ "Zmniejsza szansę paraliżującej trucizny", "X%", 384, 602, null, null, null, null, null, null }},
            { 67, new List<object>{ "Zmniejsza szansę wszystkich złych efektów", "X%", null, null, null, null, null, null, 285, 402 }},
            { 68, new List<object>{ "Zwiększa przywracanie PŻ podczas odpoczywania", "X%", 384, 604, 490, 1004, null, null, null, null }},
            { 69, new List<object>{ "Zwiększa naturalne przywracanie PŻ", "X%", null, null, null, null, 482, 1002, null, null }},
            { 70, new List<object>{ "Zwiększa regenerację PM w stanie spoczynku", "X%", 384, 604, 490, 1004, null, null, null, null }},
            { 71, new List<object>{ "Zwiększa naturalne przywracanie PM", "X%", null, null, null, null, 482, 1002, null, null }},
            { 72, new List<object>{ "Przywracanie PŻ podczas obrony", "X%", null, null, null, null, null, null, 384, 702 }},
            { 73, new List<object>{ "Zmniejsza szansę otrzymania krytycznego uderzenia", "X%", 46, 102, 110, 167, null, null, null, null }},
            { 74, new List<object>{ "Zwiększa odporność na ogień", "X%", null, null, 60, 104, 88, 204, null, null }},
            { 75, new List<object>{ "Zwiększa odporność na wodę", "X%", null, null, 60, 104, 88, 204, null, null }},
            { 76, new List<object>{ "Zwiększa odporność na światło", "X%", null, null, 60, 104, 88, 204, null, null }},
            { 77, new List<object>{ "Zwiększa odporność na mrok", "X%", null, null, 60, 104, 88, 204, null, null }},
            { 78, new List<object>{ "Zwiększa ogólną odporność", "X%", null, null, null, null, null, null, 201, 278 }},
            { 79, new List<object>{ "Diminution de la réduction de l\"honneur", "X%", 510, 590, null, null, null, null, null, null }},
            { 80, new List<object>{ "Diminue les points de consommation", "X%", 508, 577, 471, 602, null, null, null, null }},
            { 81, new List<object>{ "Augmente la production du Mini-Jeu", "X%", null, null, 490, 602, 693, 904, null, null }},
            { 82, new List<object>{ "La nourriture guérie mieux", "X%", null, null, 187, 290, 482, 604, null, null }},
            { 83, new List<object>{ "% pour toutes les défenses en PVP", "X%", 88, 118, 125, 177, 187, 252, 273, 404 }},
            { 84, new List<object>{ "Évite les attaques au corps en PVP", "X%", null, null, 39, 78, 88, 145, null, null }},
            { 85, new List<object>{ "Évite les attaques à distance en PVP", "X%", null, null, 39, 78, 88, 145, null, null }},
            { 86, new List<object>{ "Ignore les dégâts des attaques magiques en PVP", "X%", null, null, 39, 78, 88, 145, null, null }},
            { 87, new List<object>{ "Esquive toutes les attaques en PVP", "X%", null, null, null, null, null, null, 130, 212 }},
            { 88, new List<object>{ "Protèges vos MP en PVP", "X", null, null, null, null, null, null, null, null }},
            { 89, new List<object>{ "Odporny na obrażenia od ognia w PVP", "X", null, null, null, null, null, null, null, null }},
            { 90, new List<object>{ "Odporny na obrażenia od wody w PVP", "X", null, null, null, null, null, null, null, null }},
            { 91, new List<object>{ "Odporny na obrażenia od światła w PVP", "X", null, null, null, null, null, null, null, null }},
            { 92, new List<object>{ "Odporny na obrażenia od mroku w PVP", "X", null, null, null, null, null, null, null, null }}
        };

        private static readonly Random Rand = new Random();

        private static int? GenerateOptionValue(int randomOptionId, int d, int shellLevel)
        {
            int optionLevel = d % 4 == 0 ? 4 : d % 4;
            int? minimum = (int?)ShellOptionType[randomOptionId][optionLevel * 2];
            int? maximum = (int?)ShellOptionType[randomOptionId][1 + optionLevel * 2];

            if (!minimum.HasValue || !maximum.HasValue)
            {
                return null;
            }

            int m = Rand.Next(minimum.Value, maximum.Value + 1);
            int value = (int)Math.Round((double)m / 1000 * shellLevel);

            if (m != 0)
            {
                return value == 0 ? 1 : value;
            }
            return null;
        }

        public static List<EquipmentOptionDTO> GenerateShell(int shellType, int shellRarity, int shellLevel)
        {
            int w = 1;
            List<int> factor;
            int letterMultiplier = 1;
            List<EquipmentOptionDTO> shellOptions = new List<EquipmentOptionDTO>();
            List<object> optionsAlreadyOn = new List<object>();

            if (shellLevel < 50)
            {
                shellLevel = 50;
            }

            if (shellType < 8)
            {
                letterMultiplier = shellLevel <= 70 ? 1 : shellLevel <= 80 ? 3 : shellLevel <= 90 ? 5 : 1;
            }
            else if (shellType < 10)
            {
                letterMultiplier = shellLevel * 2 + 1;
            }

            switch (shellType)
            {
                case 0:
                    w = 1 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 1:
                    w = 8 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 2:
                    w = 43 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 3:
                    w = 50 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 4:
                    w = 85 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 5:
                    w = 92 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 6:
                    w = 127 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 7:
                    w = 134 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 8:
                    w = 169 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 9:
                    w = 176 + 7 * letterMultiplier - 8 + shellRarity;
                    break;
                case 10:
                    factor = new List<int> { 1, 43, 85, 127 };
                    w = factor[Rand.Next(factor.Count)] + shellRarity;
                    break;
                case 11:
                    factor = new List<int> { 8, 50, 92, 134 };
                    w = factor[Rand.Next(factor.Count)] + shellRarity;
                    break;
                default:
                    throw new Exception("Incorrect shellType");
            }

            for (int g = 3; g <= 23; g += 2)
            {
                if (ShellType[w][g] == null)
                {
                    continue;
                }

                List<object> possibleOptions = new List<object>();

                for (int i = 1; i <= 20; ++i)
                {
                    int? index = (int?)ShellType[w][g];
                    if (index == null)
                    {
                        continue;
                    }

                    object option = ShellOptionLevel[index.Value]?[i];
                    if (option == null)
                    {
                        continue;
                    }
                    possibleOptions.Add(option);
                }

                foreach (object t in optionsAlreadyOn)
                {
                    if (possibleOptions.Contains(t))
                    {
                        possibleOptions.Remove(t);
                    }
                }

                if (possibleOptions.Count == 0)
                {
                    continue;
                }

                object generatedOption = possibleOptions[Rand.Next(possibleOptions.Count)];

                if ((int)ShellType[w][g + 1] != 1 && Rand.Next(2) != 0)
                {
                    continue;
                }

                int? optionValue = GenerateOptionValue((int)generatedOption, (int)ShellType[w][g], shellLevel);
                int optionType = (int)generatedOption;
                int optionLevel = (int)ShellType[w][g];

                if (optionValue == null)
                {
                    continue;
                }

                optionsAlreadyOn.Add(generatedOption);
                shellOptions.Add(new EquipmentOptionDTO
                {
                    Level = (byte)optionLevel,
                    Type = (byte)optionType,
                    Value = (int)optionValue
                });
            }
            return shellOptions;
        }
    }
}