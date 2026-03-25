using matchmaking.Domain;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace matchmaking.Repositories
{
    internal class PhotoRepository
    {
        private readonly string _connectionString;

        public PhotoRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private Photo MapPhoto(SqlDataReader reader)
        {
            int id = (int)reader["photoId"];
            int userId = (int)reader["userId"];
            string location = (string)reader["location"];

            return new Photo(id, userId, location);
        }

        public List<Photo> GetAll()
        {
            const string query = @"SELECT photoId, userId, [location] FROM Photos";

            List<Photo> photos = new List<Photo>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                photos.Add(MapPhoto(reader));
            }

            return photos;
        }

        public void Add(Photo photo)
        {
            const string query = @"INSERT INTO Photos (userId, [location]) VALUES (@userId, @location)";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@userId", photo.UserId);
            command.Parameters.AddWithValue("@location", photo.Location);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public Photo? DeleteById(int photoId)
        {
            const string query = @"
                DELETE FROM Photos 
                OUTPUT DELETED.photoId, DELETED.userId, DELETED.[location]
                WHERE photoId = @photoId;";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@photoId", photoId);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapPhoto(reader);
            }

            return null;
        }

        public Photo? FindById(int photoId)
        {
            const string query = @"SELECT photoId, userId, [location] FROM Photos WHERE photoId = @photoId";

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@photoId", photoId);

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                return MapPhoto(reader);
            }

            return null;
        }

        public List<Photo> GetByUserId(int userId)
        {
            const string query = @"SELECT photoId, userId, [location] FROM Photos WHERE userId = @userId";

            List<Photo> photos = new List<Photo>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@userId", userId);

            connection.Open();
            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                photos.Add(MapPhoto(reader));
            }

            return photos;
        }

        public List<Photo> DeleteByUserId(int userId)
        {
            const string query = @"
                DELETE FROM Photos 
                OUTPUT DELETED.photoId, DELETED.userId, DELETED.[location]
                WHERE userId = @userId;";

            List<Photo> deletedPhotos = new List<Photo>();

            using SqlConnection connection = new SqlConnection(_connectionString);
            using SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@userId", userId);

            connection.Open();

            using SqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                deletedPhotos.Add(MapPhoto(reader));
            }

            return deletedPhotos;
        }
    }
}
