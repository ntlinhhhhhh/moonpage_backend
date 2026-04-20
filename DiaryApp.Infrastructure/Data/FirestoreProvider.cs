using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;

namespace DiaryApp.Infrastructure.Data;

public class FirestoreProvider
{
    public FirestoreDb Database { get; }

        public FirestoreProvider(FirestoreDb firestoreDb)
    {
        // Gán nó vào biến của class
        Database = firestoreDb;
    }
}