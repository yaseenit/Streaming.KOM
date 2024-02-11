from setuptools import setup, find_packages

def read_requirements(path: str) -> list[str]:
    """Read the package requirements from disk an return them as a list

    :param path: The path to the requirements file
    :return: A list of reqirements
    """
    with open(path, "r") as file:
        return file.readlines()

setup(
    name="pointcloudtool",
    version="0.7.1",
    description="A tool to handle point clouds",
    author="tgru",
    author_email="21686590+tgru@users.noreply.github.com",    
    packages=find_packages("."),
    install_requires=read_requirements("requirements.txt"),
    extras_require={
        "dev": read_requirements("requirements.dev.txt"),
    },
    entry_points={
        'console_scripts': [
            'pointcloudtool = pointcloudtool.app:main',
        ],
    },
)