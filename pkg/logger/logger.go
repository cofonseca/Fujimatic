package logger

import (
	"fmt"
	"log"
	"os"
)

var (
	infoLogger  *log.Logger
	errorLogger *log.Logger
)

func init() {
	infoLogger = log.New(os.Stdout, "INFO: ", log.Ldate|log.Ltime)
	errorLogger = log.New(os.Stderr, "ERROR: ", log.Ldate|log.Ltime|log.Lshortfile)
}

// Info logs an informational message
func Info(format string, v ...interface{}) {
	infoLogger.Printf(format, v...)
}

// Error logs an error message
func Error(format string, v ...interface{}) {
	errorLogger.Printf(format, v...)
}

// Fatal logs an error message and exits
func Fatal(format string, v ...interface{}) {
	errorLogger.Printf(format, v...)
	os.Exit(1)
}

// Print is a simple print function (no timestamp)
func Print(format string, v ...interface{}) {
	fmt.Printf(format, v...)
}
