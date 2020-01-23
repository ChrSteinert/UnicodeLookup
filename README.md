# SAFE Example – Unicode Analyzer

## Build

First, build the Frontend:
```sh
yarn webpack -p
```

This builds the `src/Client` files (F#→JS).

Then run the Backend and Frontend server:
```sh
dotnet run --project .\src\Analyzer\Analyzer.fsproj
```

Visit http://localhost:8080 (in Chrome…)
