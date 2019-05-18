﻿using Dapper;
using Npgsql;
using ResourceServer.JSONModels;
using ResourceServer.Resources;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace ResourceServer.Models
{
    public class TrueHomeContext
    {
        private static String query;
        //Get User by Login
        public static User getUserByLogin(string login)
        {
            query = $"SELECT * FROM \"user\" WHERE Login = {login};";

            User user = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                user = connection.Query<User>(query).FirstOrDefault();
            }
            return user;
        }

        public static IList<Apartment> getUserApartmentList(string userID)
        {
            query = "SELECT * FROM Apartment " +
                    $"WHERE IDUser = '{userID}'";

            IList<Apartment> apartmentList = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                apartmentList = connection.Query<Apartment>(query).ToList();
            }

            return apartmentList;
        }
        //Get User by ID
        public static User getUser(string userID)
        {
            query = $"SELECT * FROM \"user\" WHERE ID_User = '{userID}';";

            User user = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                user = connection.Query<User>(query).FirstOrDefault();
            }
            return user;
        }

        //Add new user
        public static async Task addUser(User user)
        {
            user.isBlocked = false;

            query = "INSERT INTO \"user\" " +
                    "(ID_User, Login, Email, isBlocked, IDRole)" +
                    "VALUES " +
                    $"('{user.ID_User}','{user.Login}','{user.Email}',{user.isBlocked},{user.IDRole});";

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                await connection.ExecuteAsync(query);
            }
        }

        public static PersonalData getPersonalDataByUserID([FromBody]string userID)
        {
            query = "SELECT * FROM PersonalData AS PD " +
                    "LEFT JOIN \"user\" AS U " +
                    "ON PD.IDUser = U.ID_User " +
                    $"WHERE PD.IDUser = '{userID}';";

            PersonalData ps = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                ps = connection.Query<PersonalData>(query).FirstOrDefault();
            }
            return ps;
        }

        //Add new user
        public static async Task addPersonalData(PersonalData personalData)
        {
            if (personalData.BirthDate == null)
            {
                personalData.BirthDate = new DateTime(1337, 4, 20);
            }

            query = "INSERT INTO PersonalData " +
                    "(FirstName, LastName, BirthDate, IDUser)" +
                    "VALUES " +
                    $"('{personalData.FirstName}','{personalData.LastName}'," +
                    $"'{personalData.BirthDate}','{personalData.IDUser}');";

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                await connection.ExecuteAsync(query);
            }
        }

        //Get Apartment by id
        public static Apartment getApartment(int id)
        {
            query = $"SELECT * FROM Apartment WHERE id_ap = {id};";

            Apartment apartment = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                apartment = connection.Query<Apartment>(query).FirstOrDefault();
            }

            return apartment;
        }

        //Get all Apartments
        public static IList<Apartment> getAllApartments()
        {
            query = "SELECT * FROM Apartment;";

            IList<Apartment> apartment = null;

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                apartment = connection.Query<Apartment>(query).ToList();
            }
            return apartment;
        }

        //Get with limit and offset Apartments
        public static ApartmentJSON getApartments(int limit, int offset)
        {
            query = $"SELECT * FROM Apartment ORDER BY ID_Ap ASC LIMIT {limit} OFFSET {offset};";

            IList<Apartment> apartments = null;
            ApartmentJSON apJson = new ApartmentJSON();

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                apartments = connection.Query<Apartment>(query).ToList();
            }

            if (apartments.Count <= limit)
            {
                apJson.hasMore = false;
                apJson.apartmentsList = apartments;
            }
            else
            {
                apartments.RemoveAt(limit);
                apJson.hasMore = true;
                apJson.apartmentsList = apartments;
            }

            return apJson;
        }

        //Update Apartment
        public static void updateApartment(Apartment ap)
        {
            query = @"UPDATE Apartment SET " +
                    "Name = @Name," +
                    "City = @City," +
                    "Street = @Street," +
                    "ApartmentNumber = @ApartmentNumber," +
                    "ImgThumb = @ImgThumb," +
                    "ImgList = @ImgList," +
                    "Rate = @Rate," +
                    "Lat = @Lat," +
                    "Long = @Long," +
                    "IDUser = @IDUser," +
                    "Description = @Description " +
                    "WHERE ID_Ap = @ID_Ap;";

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                connection.Execute(query, ap);
            }
        }

        public static async Task<bool> UpdateApartmentAsync(Apartment ap)
        {
            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                return await connection.UpdateAsync(ap);
            }
        }

        //Create Apartment
        public static async Task<int> createApartment(Apartment ap)
        {
            query = @"INSERT INTO Apartment " +
                    "(Name,City,Street,ApartmentNumber,ImgThumb,ImgList,Rate,Lat,Long,IDUser)" +
                    " VALUES " +
                    "(@Name,@City,@Street,@ApartmentNumber,@ImgThumb,@ImgList,@Rate,@Lat,@Long,@IDUser)" +
                    "RETURNING ID_Ap";

            int id;
            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                id = await connection.ExecuteScalarAsync<int>(query, ap);
            }

            return id;
        }
        //Delete Apartment
        public static void deleteApartment(int id)
        {
            query = "DELETE FROM Apartment" +
                    $" WHERE id_ap = {id};";

            using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            {
                connection.Open();
                connection.Execute(query);
            }
        }
        //Add picture reference
        public static void AddPictureRef(int id, string fileName)
        {
            var apartment = getApartment(id);

            if (apartment.ImgList == null)
            {
                apartment.ImgList = new[] {fileName};
                apartment.ImgThumb = fileName;
            }
            else
                apartment.ImgList = apartment.ImgList
                    ?.Concat(new[] {fileName}).ToArray();

            updateApartment(apartment);

            //TODO: make this work instead of loading whole apartment object
            //query = @"SELECT ImgList FROM Apartment WHERE id_ap = @id;";
            //var updQuery = @"UPDATE Apartment SET ImgList = @imgList WHERE id_ap = @id;";

            //using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            //{
            //    connection.Open();
            //    var imgList = connection.Query<string[]>(query, new {id}).FirstOrDefault();
            //    imgList.Append(fileName);
            //    connection.Execute(updQuery, new {imgList, id});
            //}
        }
        //Delete picture reference
        public static void DeletePictureRef(int id, string fileName)
        {
            var apartment = getApartment(id);
            apartment.ImgList = apartment.ImgList.Where(file => file != fileName).ToArray();
            updateApartment(apartment);

            //TODO: make this work instead of loading whole apartment object
            //query = @"SELECT ImgList FROM Apartment WHERE id_ap = @id;";
            //var updQuery = @"UPDATE Apartment SET ImgList = @imgList WHERE id_ap = @id;";

            //using (var connection = new NpgsqlConnection(AppSettingProvider.connString))
            //{
            //    connection.Open();
            //    var imgList = connection.Query<string[]>(query, new {id}).FirstOrDefault();
            //    imgList.Append(fileName);
            //    connection.Execute(updQuery, new {imgList, id});
            //}
        }
    }
}
