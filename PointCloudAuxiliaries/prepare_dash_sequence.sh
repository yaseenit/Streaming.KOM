#!/bin/bash

program=$(basename ${0})
version="0.1.0"

### Setup BEGIN ###
# - draco_encoder and draco_decoder must be build with
#   https://github.com/google/draco Add the build folder containing the
#   binary to the system path.
# 
# - PointCloudTool must be installed for dedublication
# 
# - PointCloudServer must be installed for MPD generation
### SETUP END ###
QT=10
QN=8
QG=8
CL=7
function help {
  echo "Prepare a Ply encoded point cloud sequence for consumption by the DASH server."
  echo "Usage: ${program} [-hv] -i DIR -o DIR -p BITS -f FPS -b URL"
  echo "-h  Show this help dialog."
  echo "-v  Show this programs version."
  echo "-i  Directory containing the source Ply files."
  echo "-o  Directory for Draco compressed files and the MPD."
  echo "-p  String of space delimted QP values to use as quality levels."
  echo "-f  Constant frames per second value for the sequence."
  echo "-b  Base URL of the DASH server."
}

function version {
  echo "${program} ${version}"
}

if [[ $# -eq 0 ]] ; then
    help
    exit 0
fi

while getopts hvi:o:p:f:b: flag
do
    case "${flag}" in
        h)
          help
          exit 0;;
        v)
          version
          exit 0;;
        i)
          INPUT_DIR=${OPTARG};;
        o)
          OUTPUT_DIR=${OPTARG};;
        p)
          QPS=${OPTARG};;
        f)
          FPS=${OPTARG};;
        b)
          BASE_URL=${OPTARG};;
    esac
done

mkdir -p ${OUTPUT_DIR}

temp_dir=${OUTPUT_DIR}/temp
mkdir -p ${temp_dir}

enc_temp_file="${temp_dir}/enc.temp.drc"
dec_temp_file="${temp_dir}/dec.temp.ply"

for qp in ${QPS}; do
    target_dir=${OUTPUT_DIR}/qp${qp}
    mkdir -p ${target_dir}

    for path in ${INPUT_DIR}/*.ply; do
        # Encode
        draco_encoder -point_cloud -i "${path}" -qp ${qp} -qt ${QT} -qn ${QN} -qg ${QG} -cl ${CL} -o ${enc_temp_file}

        # Decode back
        draco_decoder -i ${enc_temp_file} -o ${dec_temp_file}

        # Dedulicate
        pointcloudtool edit --inputPath ${dec_temp_file} --outputPath ${dec_temp_file} --dedublicate --dropNormals --addAlphaChannel

        # Encode back
        target_file=$(basename $path)
        target_file=${target_file%.ply}.drc
        draco_encoder -point_cloud -i ${dec_temp_file} -qp ${qp} -qt ${QT} -qn ${QN} -qg ${QG} -cl ${CL} -o ${target_dir}/${target_file}

    done
done

# Cleanup
echo "Cleaning up temporary files"
rm -r ${temp_dir}

echo "Generating MPD file"
pointcloudserver mpd --pretty --outputFile ${OUTPUT_DIR}/mpd.xml --framesPerSecond ${FPS} --baseUrl ${BASE_URL} ${OUTPUT_DIR}