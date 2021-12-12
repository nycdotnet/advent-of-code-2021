using adventofcode2021_dec08;

var display = new SevenSegmentDisplay();

for (var i = 0; i < 10; i++)
{
    display.SetNumber(i);
    display.ConsoleRender();
    Console.WriteLine();
}
