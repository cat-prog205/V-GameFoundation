#nullable enable
     using BlueprintConfig = BlueprintFlow.BlueprintControlFlow.BlueprintConfig;
     using BlueprintDownloader = BlueprintFlow.APIHandler.BlueprintDownloader;
     using BlueprintReaderManager = BlueprintFlow.BlueprintControlFlow.BlueprintReaderManager;
     using FetchBlueprintInfo = BlueprintFlow.APIHandler.FetchBlueprintInfo;
     using IGenericBlueprintReader = BlueprintFlow.BlueprintReader.IGenericBlueprintReader;

     namespace GameFoundation.BlueprintFlow
     {
          using UnityEngine;
          using VContainer;
          using VGameFoundation.DI;
          using VGameFoundation.Scripts.Utilities.Extension;

          public static class BlueprintsInstaller
          {
               public static void RegisterBlueprints(this IContainerBuilder builder)
               {
                    // builder.Register<PreProcessBlueprintMobile>(Lifetime.Singleton);
                    builder.Register<FetchBlueprintInfo>(Lifetime.Singleton);
                    builder.Register<BlueprintDownloader>(Lifetime.Singleton);
                    builder.Register<BlueprintReaderManager>(Lifetime.Singleton);
                    // builder.Register<BlueprintConfig>( Lifetime.Singleton).Construct(scope);
                    builder.RegisterInstance(Resources.Load<BlueprintConfig>("GameConfigs/BlueprintConfig"));

                    typeof(IGenericBlueprintReader).GetDerivedTypes().
                                                    ForEach(type =>
                                                         builder.Register(type, Lifetime.Singleton).
                                                                 AsInterfacesAndSelf());
               }
          }
     }
