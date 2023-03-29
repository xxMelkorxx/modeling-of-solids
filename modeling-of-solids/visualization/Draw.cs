using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace modeling_of_solids
{
	class Draw
	{
		public static void CreateSphere(Model3DGroup modelGroup, Vector center, double radius, int rowCount, int columnCount)
		{
			MeshGeometry3D mesh = new();
			DiffuseMaterial diffuseMaterial = new(new SolidColorBrush(Colors.Blue));

			double phi0, theta0;
			double dphi = Math.PI / rowCount;
			double dtheta = 2 * Math.PI / columnCount;

			phi0 = 0;
			double y0 = radius * Math.Cos(phi0);
			double r0 = radius * Math.Sin(phi0);

			for (int i = 0; i < rowCount; i++)
			{
				double phi1 = phi0 + dphi;
				double y1 = radius * Math.Cos(phi1);
				double r1 = radius * Math.Sin(phi1);

				theta0 = 0;
				Point3D pt00 = new(center.X + r0 * Math.Cos(theta0), center.Y + y0, center.Z + r0 * Math.Sin(theta0));
				Point3D pt10 = new(center.X + r1 * Math.Cos(theta0), center.Y + y1, center.Z + r1 * Math.Sin(theta0));

				for (int j = 0; j < columnCount; j++)
				{
					double theta1 = theta0 + dtheta;
					Point3D pt01 = new(center.X + r0 * Math.Cos(theta1), center.Y + y0, center.Z + r0 * Math.Sin(theta1));
					Point3D pt11 = new(center.X + r1 * Math.Cos(theta1), center.Y + y1, center.Z + r1 * Math.Sin(theta1));

					AddTriangle(mesh, pt00, pt11, pt10);
					AddTriangle(mesh, pt00, pt01, pt11);

					theta0 = theta1;
					pt00 = pt01;
					pt10 = pt11;
				}

				phi0 = phi1;
				y0 = y1;
				r0 = r1;
			}

			GeometryModel3D geometryModel3D = new(mesh, diffuseMaterial);
			modelGroup.Children.Add(geometryModel3D);
		}

		public static void AddTriangle(MeshGeometry3D mesh, Point3D point1, Point3D point2, Point3D point3)
		{
			int i = mesh.Positions.Count;

			mesh.Positions.Add(point1);
			mesh.Positions.Add(point2);
			mesh.Positions.Add(point3);

			mesh.TriangleIndices.Add(i++);
			mesh.TriangleIndices.Add(i++);
			mesh.TriangleIndices.Add(i);
		}

		public static void CreateCube(Model3DGroup modelGroup, Vector center, double l)
		{
			// Создаем трехмерный куб
			MeshGeometry3D cubeMesh = new MeshGeometry3D();

			// Создаем вершины куба
			cubeMesh.Positions.Add(new(center.X - l / 2, center.Y - l / 2, center.Z - l / 2));	// 0
			cubeMesh.Positions.Add(new(center.X - l / 2, center.Y - l / 2, center.Z + l / 2));	// 1
			cubeMesh.Positions.Add(new(center.X - l / 2, center.Y + l / 2, center.Z - l / 2));	// 2
			cubeMesh.Positions.Add(new(center.X - l / 2, center.Y + l / 2, center.Z + l / 2));	// 3
			cubeMesh.Positions.Add(new(center.X + l / 2, center.Y - l / 2, center.Z - l / 2));	// 4
			cubeMesh.Positions.Add(new(center.X + l / 2, center.Y - l / 2, center.Z + l / 2));	// 5
			cubeMesh.Positions.Add(new(center.X + l / 2, center.Y + l / 2, center.Z - l / 2));	// 6
			cubeMesh.Positions.Add(new(center.X + l / 2, center.Y + l / 2, center.Z + l / 2));	// 7

			// Создаем треугольники для каждой грани куба
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(1);
			cubeMesh.TriangleIndices.Add(3);
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(3);
			cubeMesh.TriangleIndices.Add(2);
			cubeMesh.TriangleIndices.Add(4);
			cubeMesh.TriangleIndices.Add(6);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(4);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(5);
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(4);
			cubeMesh.TriangleIndices.Add(5);
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(5);
			cubeMesh.TriangleIndices.Add(1);
			cubeMesh.TriangleIndices.Add(2);
			cubeMesh.TriangleIndices.Add(3);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(2);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(6);
			cubeMesh.TriangleIndices.Add(1);
			cubeMesh.TriangleIndices.Add(5);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(1);
			cubeMesh.TriangleIndices.Add(7);
			cubeMesh.TriangleIndices.Add(3);
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(2);
			cubeMesh.TriangleIndices.Add(6);
			cubeMesh.TriangleIndices.Add(0);
			cubeMesh.TriangleIndices.Add(6);
			cubeMesh.TriangleIndices.Add(4);

			// Создаем материал куба с прозрачным эффектом
			DiffuseMaterial cubeMaterial = new()
			{
				Brush = new SolidColorBrush(Color.FromArgb(128, 200, 200, 200))
				//Brush = Brushes.Transparent
			};

			// Создаем модель куба
			GeometryModel3D cubeModel = new()
			{
				Geometry = cubeMesh,
				Material = cubeMaterial
			};

			modelGroup.Children.Add(cubeModel);
		}
	}
}