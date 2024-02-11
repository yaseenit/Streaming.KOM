import open3d as o3d

def render_pointcloud(pointcloud: o3d.geometry.PointCloud, window_name: str="", point_size: float=1.0) -> None:
    """Render a point cloud in a window.

    :param pointcloud: Point cloud to render
    :param window_name: Name displayed in the window title. Empty by default
    :param point_size: Point size to use for rendering, defaults to 1.0
    """
    vis = o3d.visualization.Visualizer() 
    vis.create_window(window_name=window_name, width=800, height=600, visible=True)
    vis.get_render_option().point_size=point_size    
    
    vis.add_geometry(pointcloud, reset_bounding_box=True)
 
    vis.run()
    vis.destroy_window()