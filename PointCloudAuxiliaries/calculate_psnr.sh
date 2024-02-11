#!/bin/bash

# PccAppMetrics can be build with https://github.com/MPEGGroup/mpeg-pcc-tmc2
# Add the build folder containing the binary to the system path
metrics_cmd="PccAppMetrics"

program=$(basename ${0})
version="0.1.0"

function help {
  echo "Calculate the PSNR values between equally named files in two directories and output them CSV formated"
  echo "Usage: ${program} [-hv] -s DIR -r DIR"
  echo "-h  Show this help dialog"
  echo "-v  Show this programs version"
  echo "-s  Directory containing the source Ply files."
  echo "-r  Directory containing the reconstructed Ply files."
}

function version {
  echo "${program} ${version}"
}

if [[ $# -eq 0 ]] ; then
    help
    exit 0
fi

while getopts hvs:r: flag
do
    case "${flag}" in
        h)
          help
          exit 0;;
        v)
          version
          exit 0;;
        s)
          seq_source_dir=${OPTARG};;
        r)
          seq_reconstructed_dir=${OPTARG};;
    esac
done

# Assumption: Respective files in source and reconstructed directory are named the same

# mse1_PSNR: "Use infile1 (A) as reference, loop over A, use normals on B. (A->B)."
# mse2_PSNR: "Use infile2 (B) as reference, loop over B, use normals on A. (B->A)."
# mseF_PSNR: "Final (symmetric)."
echo "file;mse1_PSNR_p2p;mse2_PSNR_p2p;mseF_PSNR_p2p"
for file in ${seq_source_dir}/*.ply; do
  seq_source_file=${seq_source_dir}/$(basename $file)
  seq_reconstructed_file=${seq_reconstructed_dir}/$(basename $file)

  psnr=$(
  ${metrics_cmd}\
    --frameCount=1\
    --uncompressedDataPath=$seq_source_file\
    --reconstructedDataPath=$seq_reconstructed_file\
    --nbThread=8
  )
  mse1_psnr=$(echo "$psnr" | grep mse1,PSNR | grep -Eo '[0-9]+[.][0-9]')
  mse2_psnr=$(echo "$psnr" | grep mse2,PSNR | grep -Eo '[0-9]+[.][0-9]')
  mseF_psnr=$(echo "$psnr" | grep mseF,PSNR | grep -Eo '[0-9]+[.][0-9]')

  echo "$(basename $file);${mse1_psnr};${mse2_psnr};${mseF_psnr}"
done