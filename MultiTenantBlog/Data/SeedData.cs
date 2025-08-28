using Microsoft.EntityFrameworkCore;
using MultiTenantBlog.Models;

namespace MultiTenantBlog.Data
{
    /// <summary>
    /// Veritabanına test verisi eklemek için kullanılır
    /// İlk kurulumda çalıştırılır
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Veritabanını test verisiyle doldurur
        /// </summary>
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Veritabanının oluşturulduğundan emin ol
            await context.Database.EnsureCreatedAsync();

            // Eğer zaten veri varsa, seeding yapma
            if (context.Tenants.Any())
            {
                return; // Database zaten seed edilmiş
            }

            Console.WriteLine("Veritabanı test verisiyle dolduruluyor...");

            // 1. TENANT'LARI OLUŞTUR
            var tenants = new List<Tenant>
            {
                new Tenant
                {
                    Name = "Tech Innovators",
                    Domain = "localhost",
                    Subdomain = "tech",
                    Description = "Teknolojinin geleceğini keşfedin - Yapay zeka, yazılım geliştirme ve innovation",
                    Theme = "Tech",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-30)
                },
                new Tenant
                {
                    Name = "Life & Style Blog",
                    Domain = "lifestyle.localhost",
                    Subdomain = "lifestyle",
                    Description = "Yaşamın renkli tarafını keşfedin - Moda, sağlık, seyahat ve lifestyle",
                    Theme = "Lifestyle",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-25)
                },
                new Tenant
                {
                    Name = "Business Hub",
                    Domain = "business.localhost",
                    Subdomain = "business",
                    Description = "İş dünyasının nabzını tutun - Girişimcilik, finans ve yönetim",
                    Theme = "Business",
                    IsActive = true,
                    CreatedDate = DateTime.Now.AddDays(-20)
                }
            };

            context.Tenants.AddRange(tenants);
            await context.SaveChangesAsync();

            Console.WriteLine($"{tenants.Count} tenant oluşturuldu.");

            // 2. KATEGORİLERİ OLUŞTUR
            var categories = new List<Category>();

            // Tech Blog Kategorileri
            var techTenant = tenants.First(t => t.Name == "Tech Innovators");
            categories.AddRange(new List<Category>
            {
                new Category
                {
                    TenantId = techTenant.Id,
                    Name = "Yapay Zeka",
                    Description = "AI, Machine Learning ve Deep Learning konuları",
                    Color = "#e74c3c"
                },
                new Category
                {
                    TenantId = techTenant.Id,
                    Name = "Web Development",
                    Description = "Frontend, Backend ve Full-Stack geliştirme",
                    Color = "#3498db"
                },
                new Category
                {
                    TenantId = techTenant.Id,
                    Name = "Mobil Geliştirme",
                    Description = "iOS, Android ve Cross-platform uygulamalar",
                    Color = "#9b59b6"
                },
                new Category
                {
                    TenantId = techTenant.Id,
                    Name = "DevOps",
                    Description = "CI/CD, Cloud, Docker ve deployment konuları",
                    Color = "#f39c12"
                }
            });

            // Lifestyle Blog Kategorileri
            var lifestyleTenant = tenants.First(t => t.Name == "Life & Style Blog");
            categories.AddRange(new List<Category>
            {
                new Category
                {
                    TenantId = lifestyleTenant.Id,
                    Name = "Moda",
                    Description = "Trendler, stil önerileri ve moda dünyası",
                    Color = "#e91e63"
                },
                new Category
                {
                    TenantId = lifestyleTenant.Id,
                    Name = "Sağlık & Fitness",
                    Description = "Sağlıklı yaşam, egzersiz ve beslenme",
                    Color = "#4caf50"
                },
                new Category
                {
                    TenantId = lifestyleTenant.Id,
                    Name = "Seyahat",
                    Description = "Gezi rehberleri, seyahat ipuçları",
                    Color = "#ff9800"
                }
            });

            // Business Blog Kategorileri
            var businessTenant = tenants.First(t => t.Name == "Business Hub");
            categories.AddRange(new List<Category>
            {
                new Category
                {
                    TenantId = businessTenant.Id,
                    Name = "Girişimcilik",
                    Description = "Startup'lar, iş fikirleri ve girişimcilik",
                    Color = "#2196f3"
                },
                new Category
                {
                    TenantId = businessTenant.Id,
                    Name = "Finans",
                    Description = "Yatırım, borsa ve finansal planlama",
                    Color = "#ff5722"
                },
                new Category
                {
                    TenantId = businessTenant.Id,
                    Name = "Dijital Pazarlama",
                    Description = "SEO, sosyal medya ve online pazarlama",
                    Color = "#795548"
                }
            });

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            Console.WriteLine($"{categories.Count} kategori oluşturuldu.");

            // 3. BLOG YAZILARI OLUŞTUR
            var blogPosts = new List<BlogPost>();

            // Tech Blog Yazıları
            var techCategories = categories.Where(c => c.TenantId == techTenant.Id).ToList();
            blogPosts.AddRange(new List<BlogPost>
            {
                new BlogPost
                {
                    TenantId = techTenant.Id,
                    CategoryId = techCategories.First(c => c.Name == "Yapay Zeka").Id,
                    Title = "2024'te Yapay Zeka Trendleri",
                    Summary = "Bu yıl yapay zeka alanında yaşanan gelişmeler ve 2025'te neler bizi bekliyor?",
                    Content = @"
                        <h2>Yapay Zeka'nın 2024 Bilançosu</h2>
                        <p>2024 yılı, yapay zeka teknolojileri için gerçekten çığır açan bir yıl oldu. ChatGPT ve benzeri dil modellerinin yaygınlaşması ile birlikte, AI artık günlük hayatımızın ayrılmaz bir parçası haline geldi.</p>
                        
                        <h3>Öne Çıkan Gelişmeler</h3>
                        <ul>
                            <li><strong>Multimodal AI:</strong> Artık AI modelleri sadece metin değil, görüntü, ses ve video ile de çalışabiliyor</li>
                            <li><strong>Code Generation:</strong> GitHub Copilot gibi araçlar yazılım geliştirme süreçlerini tamamen değiştirdi</li>
                            <li><strong>AI Agents:</strong> Otonom olarak görevleri yerine getiren AI ajanlar gelişti</li>
                        </ul>
                        
                        <h3>2025'te Bizi Neler Bekliyor?</h3>
                        <p>Gelecek yıl yapay zeka alanında daha da heyecan verici gelişmeler yaşanacak. Özellikle:</p>
                        <p><em>Personalized AI assistants, AGI'ye doğru adımlar ve edge computing'de AI uygulamaları...</em></p>
                    ",
                    Author = "Ahmet Yılmaz",
                    PublishedDate = DateTime.Now.AddDays(-5),
                    IsPublished = true
                },
                new BlogPost
                {
                    TenantId = techTenant.Id,
                    CategoryId = techCategories.First(c => c.Name == "Web Development").Id,
                    Title = ".NET 9 ile Modern Web Uygulamaları",
                    Summary = ".NET 9'un getirdiği yenilikleri keşfedin ve modern web uygulamaları geliştirmeye başlayın.",
                    Content = @"
                        <h2>.NET 9: Yeni Nesil Web Development</h2>
                        <p>Microsoft'un en yeni .NET sürümü olan .NET 9, web geliştirme dünyasında yeni standartlar belirliyor.</p>
                        
                        <h3>Temel Özellikler</h3>
                        <p><strong>Performance Improvements:</strong> Önceki sürümlere göre %30 daha hızlı</p>
                        <p><strong>Minimal APIs:</strong> Daha az kod ile daha güçlü API'lar</p>
                        <p><strong>Hot Reload:</strong> Geliştirme sırasında anlık değişiklik görebilme</p>
                        
                        <h3>Örnek Proje</h3>
                        <p>Bu blog sistemi de .NET 9 kullanılarak geliştirildi! Multi-tenant mimarisi ile profesyonel bir blog platformu oluşturduk.</p>
                    ",
                    Author = "Zeynep Kaya",
                    PublishedDate = DateTime.Now.AddDays(-3),
                    IsPublished = true
                },
                new BlogPost
                {
                    TenantId = techTenant.Id,
                    CategoryId = techCategories.First(c => c.Name == "DevOps").Id,
                    Title = "Docker ile Microservices",
                    Summary = "Containerization teknolojileri ve microservices mimarisinin avantajları",
                    Content = @"
                        <h2>Modern Uygulama Mimarisi</h2>
                        <p>Günümüzde büyük ölçekli uygulamalar artık monolithic yapılardan uzaklaşıp, microservices mimarisine geçiyor.</p>
                        
                        <p><strong>Docker'ın Avantajları:</strong></p>
                        <p>- Tutarlı geliştirme ortamları<br/>
                        - Kolay deployment<br/>
                        - Ölçeklenebilirlik<br/>
                        - İzolasyon</p>
                        
                        <p>Bu teknolojileri öğrenmek, modern bir yazılım geliştirici için artık zorunluluk haline geldi.</p>
                    ",
                    Author = "Mehmet Demir",
                    PublishedDate = DateTime.Now.AddDays(-1),
                    IsPublished = true
                }
            });

            // Lifestyle Blog Yazıları
            var lifestyleCategories = categories.Where(c => c.TenantId == lifestyleTenant.Id).ToList();
            blogPosts.AddRange(new List<BlogPost>
            {
                new BlogPost
                {
                    TenantId = lifestyleTenant.Id,
                    CategoryId = lifestyleCategories.First(c => c.Name == "Moda").Id,
                    Title = "2024 Sonbahar Moda Trendleri",
                    Summary = "Bu sonbaharda hangi renkler, hangi kesimler moda olacak? İşte tüm detaylar!",
                    Content = @"
                        <h2>Sonbahar 2024 Moda Rehberi</h2>
                        <p>Yeni sezon geldi ve gardırobunuzu yenileme zamanı! Bu sonbaharda öne çıkacak trendleri sizler için derledik.</p>
                        
                        <h3>Renk Paletonuzu Zenginleştirin</h3>
                        <p><strong>Dominant Renkler:</strong></p>
                        <p>- Terrakota ve toprak tonları<br/>
                        - Derin yeşil ve orman yeşili<br/>
                        - Sıcak bordo ve şarap kırmızısı</p>
                        
                        <h3>Kesim ve Stiller</h3>
                        <p>Oversized ceketler bu sezonun gözdesi. Özellikle boyfriend blazer'lar hem rahat hem de şık bir görünüm sağlıyor.</p>
                    ",
                    Author = "Elif Özkan",
                    PublishedDate = DateTime.Now.AddDays(-4),
                    IsPublished = true
                },
                new BlogPost
                {
                    TenantId = lifestyleTenant.Id,
                    CategoryId = lifestyleCategories.First(c => c.Name == "Sağlık & Fitness").Id,
                    Title = "Evde 15 Dakikalık Egzersiz Rutini",
                    Summary = "Yoğun günlük tempoda fit kalmak için pratik egzersiz önerileri",
                    Content = @"
                        <h2>Zaman Yok Bahanesi Artık Geçersiz!</h2>
                        <p>Günde sadece 15 dakikanızı ayırarak formda kalabilir ve enerjinizi artırabilirsiniz.</p>
                        
                        <h3>Hızlı Egzersiz Programı</h3>
                        <p><strong>1. Isınma (3 dakika):</strong><br/>
                        - Yerinde yürüyüş<br/>
                        - Kol çevirme hareketleri</p>
                        
                        <p><strong>2. Ana Egzersizler (10 dakika):</strong><br/>
                        - 2 dakika squat<br/>
                        - 2 dakika şınav<br/>
                        - 2 dakika plank<br/>
                        - 2 dakika mountain climber<br/>
                        - 2 dakika burpee</p>
                        
                        <p><strong>3. Soğuma (2 dakika):</strong><br/>
                        - Germe egzersizleri</p>
                    ",
                    Author = "Dr. Can Yılmaz",
                    PublishedDate = DateTime.Now.AddDays(-2),
                    IsPublished = true
                }
            });

            // Business Blog Yazıları
            var businessCategories = categories.Where(c => c.TenantId == businessTenant.Id).ToList();
            blogPosts.AddRange(new List<BlogPost>
            {
                new BlogPost
                {
                    TenantId = businessTenant.Id,
                    CategoryId = businessCategories.First(c => c.Name == "Girişimcilik").Id,
                    Title = "Startup Kurmadan Önce Bilmeniz Gerekenler",
                    Summary = "İlk şirketinizi kurmadan önce dikkat etmeniz gereken 10 önemli nokta",
                    Content = @"
                        <h2>Girişimcilik Yolculuğunuz Başlamadan Önce</h2>
                        <p>Bir startup kurmak heyecan verici ama aynı zamanda zorlu bir süreç. İşte başlamadan önce bilmeniz gerekenler:</p>
                        
                        <h3>1. Pazar Araştırması</h3>
                        <p>Ürününüz için gerçek bir talep var mı? Müşteri geliştirme sürecini ihmal etmeyin.</p>
                        
                        <h3>2. Finansal Planlama</h3>
                        <p>En az 18 aylık nakit akışınızı planlayın. Beklenmedik giderler mutlaka olacak.</p>
                        
                        <h3>3. Doğru Ekip</h3>
                        <p>Teknik bilgiden çok, aynı vizyonu paylaşan insanlarla çalışmak önemli.</p>
                    ",
                    Author = "Emre Aydın",
                    PublishedDate = DateTime.Now.AddDays(-6),
                    IsPublished = true
                },
                new BlogPost
                {
                    TenantId = businessTenant.Id,
                    CategoryId = businessCategories.First(c => c.Name == "Dijital Pazarlama").Id,
                    Title = "2024'te SEO Stratejileri",
                    Summary = "Google'ın algoritma güncellemeleri sonrası SEO'da nelere odaklanmalı?",
                    Content = @"
                        <h2>SEO'nun Değişen Yüzü</h2>
                        <p>Google'ın AI odaklı güncellemeleri ile SEO stratejilerimizi de güncellemek zorundayız.</p>
                        
                        <h3>2024'ün Öne Çıkan SEO Trendleri</h3>
                        <p><strong>1. E-A-T (Expertise, Authority, Trust):</strong><br/>
                        İçerik kalitesi ve uzman otoritesi Google için daha da önemli hale geldi.</p>
                        
                        <p><strong>2. Core Web Vitals:</strong><br/>
                        Sayfa hızı ve kullanıcı deneyimi ranking faktörü olmaya devam ediyor.</p>
                        
                        <p><strong>3. AI-Generated Content:</strong><br/>
                        Yapay zeka ile oluşturulan içerikleri Google nasıl değerlendiriyor?</p>
                    ",
                    Author = "Selin Güler",
                    PublishedDate = DateTime.Now.AddDays(-7),
                    IsPublished = true
                }
            });

            context.BlogPosts.AddRange(blogPosts);
            await context.SaveChangesAsync();

            Console.WriteLine($"{blogPosts.Count} blog yazısı oluşturuldu.");
            Console.WriteLine("✅ Veritabanı test verisiyle başarıyla dolduruldu!");
        }
    }
}