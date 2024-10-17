# Process Watcher  

![GitHub repo size](https://img.shields.io/github/repo-size/jirikostiha/process-watcher)
![GitHub code size](https://img.shields.io/github/languages/code-size/jirikostiha/process-watcher)  
[![Build](https://github.com/jirikostiha/process-watcher/actions/workflows/build.yml/badge.svg)](https://github.com/jirikostiha/process-watcher/actions/workflows/build.yml)
[![Code Lint](https://github.com/jirikostiha/process-watcher/actions/workflows/lint-code.yml/badge.svg)](https://github.com/jirikostiha/process-watcher/actions/workflows/lint-code.yml)


## Overview

Process Watcher is a tool designed to monitor and track the execution a process on your system. By using this tool, users can gain basic insights into the behavior of running process and the resource consumption by it. The watched process can be automatically restarted in case of failure.

## Features

* Monitor running process in real-time

* Restarting the process if it was terminated


## Usage

Modify appsettings.json to your needs.

```json
 "ProcessWatching": {
    "CheckingPeriod": "00:00:05"
  },
  "Watchdog": {
    "ProcessFile": "c:/temp/ProcessName.exe",
    "StartDelay": "00:00:02",
    "StartWindow": "00:00:10",
    "DelayCoef": 1.5
  }
```

**ProcessWatching**  
- *CheckingPeriod*: How often the process is checked.  

**Watchdog**  
- *ProcessFile*: Path to the process executable to monitor.  
- *StartDelay*: Time to wait before starting to check the process.  
- *StartWindow*: Time window within which the process must start.  
- *DelayCoef*: Multiplier for the delay between restart attempts if the process fails.  


## Contributing

Feel free to submit issues or pull requests. All contributions are welcome!

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.