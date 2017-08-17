# Analog To Square Wave Generator

Generates Console.Beep commands for a given audio file. Currently it sounds more like a bad impersonation of R2D2...

Goertzel algorithm was used to detect frequencies and behaves very well when the changes in frequency are uniform and
continuous, however it fails to capture key features in more complex audio signals.