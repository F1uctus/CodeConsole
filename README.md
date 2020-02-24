<h1 align="center">CodeConsole</h1>

My own toolset to make console interaction in C# much more neat and fancy.

This includes:
- A bunch of helper functions (`ConsoleUtils` class, to simplify colorful output and some other stuff);
- ScriptBench - a simple, though powerful console code editor with custom highlighting support.
  + `ISyntaxHighlighter` interface is also for that purpose.

Here it is, working as a part of Axion toolset:
![ScriptBench](/showoff.png)

### Caveats

ScriptBench doesn't support console resizing for now, it'll crash if you reduce console size while editing.
