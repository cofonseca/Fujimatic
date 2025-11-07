package main

import (
    "fmt"
    "bufio"
    "os"
    "strings"
)

func main() {
    fmt.Println("Fujimatic REPL (prototype)")
    reader := bufio.NewReader(os.Stdin)
    for {
        fmt.Print("> ")
        cmd, _ := reader.ReadString('\n')
        cmd = strings.TrimSpace(cmd)
        switch cmd {
        case "exit", "quit":
            fmt.Println("Exiting...")
            return
        case "connect":
            fmt.Println("Connecting to camera (mock)...")
        case "capture":
            fmt.Println("Capturing image (mock)...")
        default:
            fmt.Println("Unknown command:", cmd)
        }
    }
}
