using adventofcode2021_dec19;
using static common.Utils;

var scanners = Scanner.ParseInput(GetLines("myPuzzleInput.txt"));




// scanners detect beacons in a cube 1000x1000x1000
// scanners can't detect other scanners.

/*
 * 
 * For example, if a scanner is at x,y,z coordinates 500,0,-500 and there are beacons
 * at -500,1000,-1500 and 1501,0,-500, the scanner could report that the first beacon
 * is at -1000,1000,-1000 (relative to the scanner) but would not detect the second beacon at all.
 * 
 * */





