from setuptools import setup, find_packages

def read_requirements(path: str) -> list[str]:
    """Read the package requirements from disk an return them as a list

    :param path: The path to the requirements file
    :return: A list of reqirements
    """
    with open(path, "r") as file:
        return file.readlines()

setup(
    name="pointcloudserver",
    version="0.4.1",
    description="A server software to stream point clouds",
    author="tgru",
    author_email="21686590+tgru@users.noreply.github.com",    
    packages=find_packages("."),
    install_requires=read_requirements("requirements.txt"),
    extras_requires={
        "dev": read_requirements("requirements-dev.txt"),
    },
    entry_points={
        'console_scripts': [
            'pointcloudserver = pointcloudserver.app:main',
        ],
    },
)