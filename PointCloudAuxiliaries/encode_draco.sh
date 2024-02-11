#!/bin/bash

# draco_encoder can be build with https://github.com/google/draco
# Add the build folder containing the binary to the system path
encode_cmd="draco_encoder"

program=$(basename ${0})
version="0.1.0"
QP=11
QT=10
QN=8
QG=8
CL=7

function help {
  echo "Encode all Ply files from a directory with Draco and save them in another one."
  echo "Usage: ${program} [-hv] [-ptng BITS] [-c LEVEL] -i DIR -o DIR"
  echo "-h  Show this help dialog."
  echo "-v  Show this programs version."
  echo "-i  Input directory containing .drc files."
  echo "-o  Output directory where reconstructed files will be saved."
  echo "-p  Quantization bits used for position values. Defaults to 11."
  echo "-t  Quantization bits used for texture coordinates. Defaults to 10."
  echo "-n  Quantization bits used for normal vectors. Defaults to 8."
  echo "-g  Quantization bits used for generic attributes. Defaults to 8."
  echo "-c  Compression level to use from 0 to 10. Defaults to 7."
}

function version {
  echo "${program} ${version}"
}

if [[ $# -eq 0 ]] ; then
    help
    exit 0
fi

while getopts hvi:o:p:t:n:g:c: flag
do
    case "${flag}" in
        h)
          help
          exit 0;;
        v)
          version
          exit 0;;
        i)
          input_dir=${OPTARG};;
        o)
          output_dir=${OPTARG};;
        p)
          QP=${OPTARG};;
        t)
          QT=${OPTARG};;
        n)
          QN=${OPTARG};;
        g)
          QG=${OPTARG};;
        c)
          CL=${OPTARG};;
    esac
done

#for i in *.ply; do
for input_path in ${input_dir}/*.ply; do
  output_path=${output_dir}/$(basename $input_path)
  output_path=${output_path%.ply}.drc # Replace extension

  $encode_cmd -point_cloud -i "${input_path}" -qp ${QP} -qt ${QT} -qn ${QN} -qg ${QG} -cl ${CL} -o ${output_path}
done
