# Insider View

Find a project file by what is inside it, not by what it is called.

## What it is

A small Windows app. Point it at a folder, give it a file type, and it lists every file of that type and reads what is inside each one in the background. Type in the search box and every file with a match gets a tick. Click a file to see everything inside it.

Right now it can look inside Adobe Illustrator `.ai` files, where it reads the names of the top-level layers. It reads them straight from the file, so Illustrator does not need to be running or even installed.

It also has:

- Show hits / Show all, to cut the list down to the files that matched
- Sorting by name, date or size
- Open in Explorer, with the file already selected
- Copy location, which puts the file's full path on the clipboard
- Copy to and Move to, for one file or many. Nothing is ever overwritten; a name that is already taken becomes `name (2)`
- Run with default app, the same as double-clicking the file

## Why I made it

I draw icons, and I have a lot of Illustrator files with stacks of icons in each one, one icon per layer. The file names tell me nothing about which icons are in which file.

I kept ending up in the same place: I know the icon exists, I just do not know which file it is in, and opening them one at a time takes forever. Now I type the layer name and it shows me the file.

## Where it is going

I will add more file types as I need to find more things. Illustrator was first because that is what I was stuck on.

Each file type is one small reader: it is handed a file and gives back the list of names inside it. Teaching the app a new type means writing that reader and adding one line to the list in `InsideReaders.cs`. Any project file that hides useful names inside it is a candidate.

## Building it

- Windows, with the .NET 10 SDK
- Open `InsiderView.slnx` in Visual Studio and run
- One package is pulled in automatically: `ZstdSharp.Port`, which unpacks the compressed drawing data inside `.ai` files

## Good to know

- It has been tested on files saved by current versions of Illustrator. The code also handles the older way Illustrator packed its files, but I have not had any to test with.
- A file type it cannot look inside yet is still found and listed. You just get no contents for it.
