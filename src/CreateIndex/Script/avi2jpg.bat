mkdir output
mkdir output\%~n1
ffmpeg -i %1 -f image2 -s 640x360 output\%~n1\%~n1_%%06d.jpg