import pytest

from pointcloudtool import io

class TestPointCloudSerializerFactory():
    def setup(self):
        self.factory = io.PointCloudSerializerFactory()

    def test_infer_format_with_windows_path(self):
        path = "C:\\path\\to\\file.ply"
        actual = self.factory.infer_format(path)

        assert actual == "ply"

    def test_infer_format_with_unix_path(self):
        path = "/path/to/file.ply"
        actual = self.factory.infer_format(path)

        assert actual == "ply"

    def test_infer_format_without_path(self):
        path = "file.ply"
        actual = self.factory.infer_format(path)

        assert actual == "ply"

    def test_infer_format_without_path_without_extension(self):
        path = "file"

        with pytest.raises(Exception):
            self.factory.infer_format(path)

file_single_ext_old = "some_file.ply"
file_single_ext_new = "some_file.drc"

file_multi_ext_old = "some.file.ply"
file_multi_ext_new = "some.file.drc"

def test_replace_extension_replaces_single_extension():
    actual = io.replace_extension(file_single_ext_old, "drc")

    assert actual == file_single_ext_new

def test_replace_extension_replaces_only_last_extension():
    actual = io.replace_extension(file_multi_ext_old, "drc")

    assert actual == file_multi_ext_new