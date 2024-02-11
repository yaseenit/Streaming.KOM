#!/bin/bash

# draco_decoder can be build with https://github.com/google/draco
# Add the build folder containing the binary to the system path
decode_cmd="draco_decoder"

program=$(basename ${0})
version="0.1.0"

function help {
  echo "Decode Draco compressed files from a directory into another"
  echo "Usage: ${program} [-hv] -i DIR -o DIR"
  echo "-h  Show this help dialog"
  echo "-v  Show this programs version"
  echo "-i  Input directory containing .drc files"
  echo "-o  Output directory where reconstructed files will be saved"
}

function version {
  echo "${program} ${version}"
}

if [[ $# -eq 0 ]] ; then
    help
    exit 0
fi

while getopts hvi:o: flag
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
    esac
done

for input_path in ${input_dir}/*.drc; do
  output_path=${output_dir}/$(basename $input_path)
  output_path=${output_path%.drc}.ply # Replace extension

  $decode_cmd -i ${input_path} -o ${output_path}
done
