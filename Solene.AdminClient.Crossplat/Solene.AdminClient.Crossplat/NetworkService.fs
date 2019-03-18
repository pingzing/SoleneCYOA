namespace Solene.AdminClient.Crossplat    

open System.Net.Http
    
type NetworkService() = 

    let httpClient = lazy (new HttpClient())
    

